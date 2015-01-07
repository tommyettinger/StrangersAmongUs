module SAUGame
open SAUActor
open MonoGameUtilities
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Media
type Game1 () as x =
    inherit Game()
 
    do x.Content.RootDirectory <- "Content"
    let graphics = new GraphicsDeviceManager(x)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let mapSize = 30.f
    let camera = new Camera()
    let CreateActor' = CreateActor x.Content
    let R = new System.Random()
    let RandomTerrain () =  List.nth ["palette50/Terrain_Huge_face0_0"; "palette51/Terrain_Huge_face0_0";] (R.Next 2)
    let RandomCreature () =  List.nth ["palette0/Human_Male_Large_face0_0"; "palette0/Human_Female_Large_face0_0"; "palette20/Lomuk_Large_face0_0";] (R.Next 3)
    let WorldObjects = lazy ([for x in 0.f .. mapSize do
                                  for y in 0.f .. mapSize do
                                      yield (RandomTerrain(), Terrain, Vector2(48.f * (x - y), 24.f * (x + y)), Vector2(168.f,208.f));]
                         |> List.sortBy (fun (_, _, v, _) -> v.Y + 0.0001f * v.X)
                         |> List.map CreateActor')

    let MobileObjects = lazy ([for x in 0.f .. mapSize do
                                  for y in 0.f .. mapSize do
                                      if R.Next(4) = 0 then
                                          yield (RandomCreature(), Player(Nothing), Vector2(40.f + 48.f * (x - y), 60.f + 24.f * (x + y)), Vector2(88.f,108.f));]
                         |> List.sortBy (fun (_, _, v, _) -> v.Y + 0.0001f * v.X)
                         |> List.map CreateActor')
                         
    let mutable font = Unchecked.defaultof<SpriteFont>
    let mutable frameRate = 0.0
    let mutable frameCounter = 0.0

    let DrawActor (sb:SpriteBatch) actor =
        if actor.Texture.IsSome then
            do sb.Draw(actor.Texture.Value, actor.Position, Color.White)
        ()

    override x.Initialize() =
        do camera.ViewportWidth <- x.GraphicsDevice.Viewport.Width;
        do camera.ViewportHeight <- x.GraphicsDevice.Viewport.Height;
        do x.Window.AllowUserResizing <- true
        do x.IsMouseVisible <- true
        x.Window.ClientSizeChanged.Add(fun e -> do camera.ViewportWidth <- x.GraphicsDevice.Viewport.Width;
                                                do camera.ViewportHeight <- x.GraphicsDevice.Viewport.Height;
                                                camera.CenterOn(Cell(mapSize/2.f, mapSize/2.f)))
        do spriteBatch <- new SpriteBatch(x.GraphicsDevice)
        do base.Initialize()
        ()
 
    override x.LoadContent() =
        camera.CenterOn(Cell(mapSize/2.f, mapSize/2.f))
        do WorldObjects.Force () |> ignore
        do MobileObjects.Force () |> ignore
        do font <- x.Content.Load("Zodiac")
        ()
 
    override x.Update (gameTime) =
        if (frameCounter > 0.0 && gameTime.ElapsedGameTime.TotalSeconds > 0.0) then
            do frameRate <- frameCounter / gameTime.ElapsedGameTime.TotalSeconds
            do frameCounter <- 0.0
        ()
 
    override x.Draw (gameTime) =
        do frameCounter <- frameCounter + 1.0;
        do x.GraphicsDevice.Clear Color.CornflowerBlue
        let DrawActor' = DrawActor spriteBatch
        do spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.TranslationMatrix)
        WorldObjects.Value @ MobileObjects.Value |> List.iter DrawActor'
        do spriteBatch.DrawString(font, (string (int frameRate)), (camera.Position), Color.OrangeRed)
        do spriteBatch.End ()
        ()