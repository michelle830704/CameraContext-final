using Godot;

// Multiple feeler rays adjust orbit position before a final line-of-sight clamp.
public partial class CameraRig : Camera3D
{
    [Export] public float Distance { get; set; } = 6;
    [Export] public float FollowSmoothing { get; set; } = 8;
    [Export] public float OrbitSpeed { get; set; } = 2.2f;
    [Export] public float AlignmentSpeed { get; set; } = 1.2f;
    [Export] public float AlignmentDelay { get; set; } = 1;
    [Export] public float MinPitch { get; set; } = -10;
    [Export] public float MaxPitch { get; set; } = 65;
    [Export] public bool ShowWhiskers { get; set; } = false;
    private CharacterBody3D player;
    private float yaw, pitch = 28, idle, sideOffset, lift;
    private Vector2 smoothedStick;
    private Vector3 focus, aim;
    private ImmediateMesh lines = new ImmediateMesh();
    private MeshInstance3D lineNode;
    public string HintName { get; private set; } = "None";
    public string Avoidance { get; private set; } = "Clear";
    public override void _Ready()
    {
        player = GetParent().GetNode<CharacterBody3D>("CharacterBody3D");
        focus = player.GlobalPosition + Vector3.Up * 0.4f; aim = focus;
        Near = 0.1f; Current = true;
        lineNode = new MeshInstance3D { Mesh = lines, TopLevel = true,
            MaterialOverride = new StandardMaterial3D { ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, VertexColorUseAsAlbedo = true } };
        AddChild(lineNode);
        GlobalPosition = focus + Offset(yaw, pitch, Distance); LookAt(focus);
    }
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        if (Input.IsPhysicalKeyPressed(Key.F2) && !toggleHeld) ShowWhiskers = !ShowWhiskers;
        toggleHeld = Input.IsPhysicalKeyPressed(Key.F2);
        Vector2 stick = Vector2.Zero;
        var pads = Input.GetConnectedJoypads();
        if (pads.Count > 0) stick = new Vector2(Input.GetJoyAxis(pads[0], JoyAxis.RightX), Input.GetJoyAxis(pads[0], JoyAxis.RightY));
        if (stick.Length() < 0.18f) stick = Vector2.Zero;
        else stick = stick.Normalized() * Mathf.Clamp((stick.Length() - 0.18f) / 0.82f, 0, 1);
        stick.X += (Input.IsPhysicalKeyPressed(Key.E) ? 1 : 0) - (Input.IsPhysicalKeyPressed(Key.Q) ? 1 : 0);
        stick.Y += (Input.IsPhysicalKeyPressed(Key.K) ? 1 : 0) - (Input.IsPhysicalKeyPressed(Key.I) ? 1 : 0);
        smoothedStick = smoothedStick.Lerp(stick.LimitLength(), Blend(12, dt));
        yaw -= smoothedStick.X * OrbitSpeed * dt;
        pitch = Mathf.Clamp(pitch + smoothedStick.Y * OrbitSpeed * 35 * dt, MinPitch, MaxPitch);
        if (stick.LengthSquared() > 0.001f) idle = 0; else idle += dt;
        Vector3 movement = new Vector3(player.Velocity.X, 0, player.Velocity.Z);
        if (idle > AlignmentDelay && movement.LengthSquared() > 0.2f)
            yaw = Mathf.LerpAngle(yaw, Mathf.Atan2(-movement.X, -movement.Z), Blend(AlignmentSpeed, dt));
        focus = focus.Lerp(player.GlobalPosition + Vector3.Up * 0.4f, Blend(FollowSmoothing, dt));
        Vector3 desired = focus + Offset(yaw, pitch, Distance);
        Vector3 targetAim = focus; float targetFov = 55;
        // Distance-based hints. Smooth weights prevent hard changes at the boundary.
        float plaza = HintWeight(player.GlobalPosition, new Vector3(0, -0.2f, -27), 6);
        float tower = HintWeight(player.GlobalPosition, new Vector3(6, -0.2f, -29), 4);
        HintName = "None";
        if (plaza > 0) {
            HintName = "Wide plaza"; targetFov = Mathf.Lerp(targetFov, 78, plaza);
            desired = desired.Lerp(focus + new Vector3(0, 5, 8), plaza);
            targetAim = targetAim.Lerp(new Vector3(0, 1, -31), plaza * 0.7f);
        }
        if (tower > 0) {
            HintName = "Tower reveal"; targetFov = Mathf.Lerp(targetFov, 42, tower);
            desired = desired.Lerp(focus + new Vector3(-5, 3, 5), tower);
            targetAim = targetAim.Lerp(new Vector3(7, 2.8f, -33), tower * 0.8f);
        }
        lines.ClearSurfaces();
        if (ShowWhiskers) lines.SurfaceBegin(Mesh.PrimitiveType.Lines);
        Vector3 right = new Vector3(Mathf.Cos(yaw), 0, -Mathf.Sin(yaw));
        float leftRisk = Risk(desired, desired - right * 1.5f);
        float rightRisk = Risk(desired, desired + right * 1.5f);
        // Forward-diagonal whiskers also detect walls before the camera reaches them.
        leftRisk = Mathf.Max(leftRisk, Risk(focus, desired - right * 0.8f));
        rightRisk = Mathf.Max(rightRisk, Risk(focus, desired + right * 0.8f));
        float groundRisk = Risk(desired, desired + Vector3.Down * 1.5f);
        float inwardRisk = Risk(focus, desired);
        sideOffset = Mathf.Lerp(sideOffset, (leftRisk - rightRisk) * 1.4f, Blend(7, dt));
        lift = Mathf.Lerp(lift, groundRisk * 1.6f, Blend(8, dt));
        desired += right * sideOffset + Vector3.Up * lift;
        desired = desired.Lerp(focus, Mathf.Clamp(groundRisk * 0.25f + inwardRisk * 0.35f, 0, 0.6f));
        Avoidance = groundRisk > 0 ? "Ground: up + inward" : inwardRisk > 0 ? "Wall: inward" : Mathf.Abs(sideOffset) > 0.05f ? "Side whiskers" : "Clear";
        // Ray bundle samples around the view, rather than just its centre.
        Vector3 constrained = Constrain(focus, desired);
        Vector3 candidate = GlobalPosition.Lerp(constrained, Blend(FollowSmoothing, dt));
        // Safety clamp after smoothing, so smoothing cannot pull the camera through a wall.
        GlobalPosition = Constrain(focus, candidate);
        if (ShowWhiskers) lines.SurfaceEnd();
        aim = aim.Lerp(targetAim, Blend(6, dt));
        if (GlobalPosition.DistanceSquaredTo(aim) > 0.01f) LookAt(aim, Vector3.Up);
        Fov = Mathf.Lerp(Fov, targetFov, Blend(5, dt));
    }
    private bool toggleHeld;
    private static float Blend(float speed, float dt) => 1 - Mathf.Exp(-speed * dt);
    private static Vector3 Offset(float y, float p, float distance) {
        float radians = Mathf.DegToRad(p);
        return new Basis(Vector3.Up, y) * new Vector3(0, Mathf.Sin(radians) * distance, Mathf.Cos(radians) * distance);
    }
    private static float HintWeight(Vector3 position, Vector3 centre, float radius) {
        float t = Mathf.Clamp(1 - position.DistanceTo(centre) / radius, 0, 1);
        return t * t * (3 - 2 * t);
    }
    private Godot.Collections.Dictionary Cast(Vector3 from, Vector3 to) {
        var query = PhysicsRayQueryParameters3D.Create(from, to, 4); // camera blockers only (layer 3)
        query.HitFromInside = true;
        var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
        if (ShowWhiskers) {
            lines.SurfaceSetColor(hit.Count > 0 ? Colors.Red : Colors.Cyan);
            lines.SurfaceAddVertex(from); lines.SurfaceAddVertex(hit.Count > 0 ? (Vector3)hit["position"] : to);
        }
        return hit;
    }
    private float Risk(Vector3 from, Vector3 to) {
        var hit = Cast(from, to);
        return hit.Count == 0 ? 0 : 1 - Mathf.Clamp(from.DistanceTo((Vector3)hit["position"]) / Mathf.Max(0.01f, from.DistanceTo(to)), 0, 1);
    }
    private Vector3 Constrain(Vector3 from, Vector3 to) {
        Vector3 travel = to - from; float length = travel.Length();
        if (length < 0.01f) return to;
        Vector3 right = travel.Normalized().Cross(Vector3.Up).Normalized();
        float safe = length;
        foreach (Vector3 offset in new Vector3[] { Vector3.Zero, right * 0.25f, -right * 0.25f, Vector3.Up * 0.25f, Vector3.Down * 0.25f }) {
            var hit = Cast(from + offset, to + offset);
            if (hit.Count > 0) safe = Mathf.Min(safe, Mathf.Max(0.15f, (from + offset).DistanceTo((Vector3)hit["position"]) - 0.3f));
        }
        return from + travel.Normalized() * safe;
    }
}
