using Godot;

// Adds the showcase beyond the original floor; original colours/materials remain intact.
public partial class CameraCourse : Node3D
{
    private Label hud;
    private CameraRig camera;
    public override void _Ready()
    {
        camera = GetNode<CameraRig>("Camera3D");
        foreach (string path in new string[] { "GROUND/StaticBody3D", "rectangle1/StaticBody3D", "MeshInstance3D2/StaticBody3D", "MeshInstance3D3/StaticBody3D" })
            GetNode<StaticBody3D>(path).CollisionLayer = 5; // player layer 1 + camera layer 3
        Box("CourseFloor", new Vector3(0, -1, -24), new Vector3(23.48f, 0.59f, 25), "GROUND", true);
        Box("LeftWall", new Vector3(-3.8f, 1.3f, -17), new Vector3(0.7f, 4, 7), "rectangle1", true);
        Box("RightWall", new Vector3(3.8f, 1.3f, -17), new Vector3(0.7f, 4, 7), "MeshInstance3D3", true);
        Box("RearWall", new Vector3(-6, 1.3f, -23), new Vector3(6, 4, 0.7f), "rectangle1", true);
        Box("SmallYellow", new Vector3(-1, -0.46f, -14), new Vector3(0.45f, 0.5f, 0.45f), "cube1", false);
        Box("SmallPink", new Vector3(1, -0.5f, -16), new Vector3(0.45f, 0.4f, 0.45f), "MeshInstance3D", false);
        Box("RaisedSurface", new Vector3(-7, 0.1f, -28), new Vector3(5, 1.6f, 6), "MeshInstance3D3", true);
        Box("StepOne", new Vector3(-7, -0.5f, -23.5f), new Vector3(4, 0.4f, 1), "cube1", true);
        Box("StepTwo", new Vector3(-7, -0.25f, -24.5f), new Vector3(4, 0.9f, 1), "cube1", true);
        Box("LandmarkTower", new Vector3(7, 2.4f, -33), new Vector3(1.5f, 6.2f, 1.5f), "cube1", true);
        Marker("Wide hint zone", new Vector3(0, -0.69f, -27), 6, "MeshInstance3D3");
        Marker("Tower hint zone", new Vector3(6, -0.68f, -29), 4, "MeshInstance3D");
        Sign("CAMERA COURSE: WALK THIS WAY", new Vector3(0, 1, -10));
        Sign("WALL WHISKERS / SMALL OBJECTS IGNORED", new Vector3(0, 3.8f, -17));
        Sign("GROUND AVOIDANCE: LOWER CAMERA WITH RIGHT STICK / I", new Vector3(-7, 2.5f, -28));
        Sign("WIDE VIEW HINT", new Vector3(0, 0.5f, -27));
        Sign("TOWER REVEAL HINT", new Vector3(6, 0.5f, -29));
        var ui = new CanvasLayer(); AddChild(ui);
        ui.AddChild(new ColorRect { Position = new Vector2(10, 10), Size = new Vector2(830, 120), Color = new Color(0, 0, 0, 0.75f), MouseFilter = Control.MouseFilterEnum.Ignore });
        hud = new Label { Position = new Vector2(20, 16) }; hud.AddThemeFontSizeOverride("font_size", 17); ui.AddChild(hud);
    }
    public override void _Process(double delta)
    {
        if (hud == null) return;
        var pads = Input.GetConnectedJoypads();
        hud.Text = "CONTEXT & CAMERA | " + (pads.Count > 0 ? Input.GetJoyName(pads[0]) : "No controller connected: keyboard preview") + "\n" +
            "Left stick: walk | Right stick: orbit | A / Cross: jump\n" +
            "Keyboard: WASD move, Q/E horizontal orbit, I/K vertical, Space jump, R reset\n" +
            $"Hint: {camera.HintName} | Avoidance: {camera.Avoidance} | FOV: {camera.Fov:0} | F2: show/hide ray whiskers\n" +
            "Walk forward beyond the original floor to reach the camera test course.";
    }
    private StandardMaterial3D Material(string original) {
        var source = GetNode<MeshInstance3D>(original);
        return source.MaterialOverride as StandardMaterial3D ?? source.GetSurfaceOverrideMaterial(0) as StandardMaterial3D ?? new StandardMaterial3D { AlbedoColor = Colors.White };
    }
    private void Box(string name, Vector3 position, Vector3 size, string source, bool cameraBlocker) {
        var body = new StaticBody3D { Name = name, Position = position, CollisionLayer = cameraBlocker ? 5u : 1u };
        body.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = size }, MaterialOverride = Material(source) });
        body.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } }); AddChild(body);
    }
    private void Marker(string name, Vector3 position, float radius, string source) {
        AddChild(new MeshInstance3D { Name = name, Position = position, Mesh = new CylinderMesh { TopRadius = radius, BottomRadius = radius, Height = 0.015f }, MaterialOverride = Material(source) });
    }
    private void Sign(string text, Vector3 position) {
        AddChild(new Label3D { Text = text, Position = position, FontSize = 36, PixelSize = 0.008f,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled });
    }
}
