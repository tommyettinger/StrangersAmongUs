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
    let WorldLogical = [for x in 0.f .. mapSize do
                                  for y in 0.f .. mapSize do
                                      for r in 0 .. R.Next(4) do
                                          yield (RandomTerrain(), Terrain, Vector2(48.f * (x - y), 24.f * (x + y) - 18.f * (float32 r)), Vector3(Vector2(x,y), (float32 r)), Vector2(168.f,208.f));]
                         |> List.map (fun (a, b, v, cv, e) -> ((cv.X, cv.Y, cv.Z), (a, b, v, cv, e)))
                         |> Map.ofList
    let HeightMap =  [for x in 0.f .. mapSize do
                          for y in 0.f .. mapSize do
                               if(WorldLogical.ContainsKey(x, y, 3.f)) then yield ((x, y), 3.f) else
                                 if(WorldLogical.ContainsKey(x, y, 2.f)) then yield ((x, y), 2.f) else
                                   if(WorldLogical.ContainsKey(x, y, 1.f)) then yield ((x, y), 1.f) else
                                     if(WorldLogical.ContainsKey(x, y, 0.f)) then yield ((x, y), 0.f)] |> Map.ofList
    
    let WorldObjects = lazy (WorldLogical
                         |> Map.toList
                         |> List.sortBy (fun ((x, y, z), (_, _, v, cv, _)) -> cv.Z * 1000.f + cv.Y + 0.0001f * cv.X))
    
    let MobileLogical = [for x in 0.f .. mapSize do
                                  for y in 0.f .. mapSize do
                                      if R.Next(4) = 0 then
                                          let z = (x, y) |> HeightMap.TryFind
                                          if z.IsSome then yield (RandomCreature(),
                                                                  Player(Nothing),
                                                                  Vector2(40.f + 48.f * (x - y), 60.f + 24.f * (x + y) - 18.f * z.Value),
                                                                  Vector3(Vector2(x,y), (float32 z.Value)),
                                                                  Vector2(88.f,108.f));]
                         |> List.map (fun (a, b, v, cv, e) -> ((cv.X, cv.Y, cv.Z), (a, b, v, cv, e)))
                         |> Map.ofList
    let MobileObjects = lazy(MobileLogical
                         |> Map.toList
                         |> List.sortBy (fun ((x, y, z), (_, _, v, cv, _)) -> cv.Z * 1000.f + cv.Y + 0.0001f * cv.X)
                         )


    let AllObjects = lazy(WorldObjects.Value @ MobileObjects.Value
                             |> List.sortBy (fun ((x, y, z), (_, _, v, cv, _)) -> cv.Z / 4.f + cv.Y + cv.X)
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
        AllObjects.Value |> List.iter DrawActor'
        //do spriteBatch.DrawString(font, (string (int frameRate)), (camera.Position), Color.OrangeRed)
        do spriteBatch.End ()
        ()