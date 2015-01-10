module SAUGame
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Media
open System.Collections.Generic

open MonoGameUtilities

open Utils
open Globals

open SAUActor

type Game1 () as x =
    inherit Game()
 
    do x.Content.RootDirectory <- "Content"
    
    let Graphics = new GraphicsDeviceManager(x)
    let MapSize = 30.f
    let GameCamera = new Camera()
    let CreateActor' = fun k -> CreateActor x.GraphicsDevice k
    let R = new System.Random()
    let RandomTerrain () =  ("Terrain", 48, RandNth [50 .. 51], RandNth ["SE"; "SW"; "NW"; "NE";])
    let RandomCreature () =
                          let (unit, sz, palette) =
                             RandNth [("Human_Male", 40, RandNth [0;1;15;16;17]);
                                      ("Human_Female", 40, RandNth [0;1;15;16;17]);
                                      ("Lomuk", 40, RandNth [20;20;20;20;10;12;]);
                                      ("Glarosp", 40, 21);
                                      ("Tassar", 40, 11);
                                      ("Sfyst", 40, 24);]
                          (unit, sz, palette, RandNth ["SE"; "SW"; "NW"; "NE";])
    let WorldLogical = [for x in 0.f .. MapSize do
                                  for y in 0.f .. MapSize do
                                      for r in 0 .. R.Next(4) do
                                          yield (RandomTerrain(),
                                                 Terrain,
                                                 Vector2(48.f * (x - y), 24.f * (x + y) - 18.f * (float32 r)),
                                                 Vector3(Vector2(x,y), (float32 r)), Vector2(88.f + 16.f,108.f + 20.f)
                                                 );]
                         |> List.map (fun (a, b, v, cv, e) -> ((cv.X, cv.Y, cv.Z), (a, b, v, cv, e)))
                         |> Map.ofList
    let HeightMap =  [for x in 0.f .. MapSize do
                          for y in 0.f .. MapSize do
                               if(WorldLogical.ContainsKey(x, y, 3.f)) then yield ((x, y), 3.f) else
                                 if(WorldLogical.ContainsKey(x, y, 2.f)) then yield ((x, y), 2.f) else
                                   if(WorldLogical.ContainsKey(x, y, 1.f)) then yield ((x, y), 1.f) else
                                     if(WorldLogical.ContainsKey(x, y, 0.f)) then yield ((x, y), 0.f)] |> Map.ofList
    
    let WorldObjects = lazy (WorldLogical
                         |> Map.toList
                         |> List.sortBy (fun ((x, y, z), (_, _, v, cv, _)) -> cv.Z * 1000.f + cv.Y + 0.0001f * cv.X))
    
    let MobileLogical = [for x in 0.f .. MapSize do
                                  for y in 0.f .. MapSize do
                                      if R.Next(4) = 0 then
                                          let z = (x, y) |> HeightMap.TryFind
                                          if z.IsSome then yield (RandomCreature(),
                                                                  Player(Nothing),
                                                                  Vector2(8.f + 48.f * (x - y), 24.f * (x + y) - 18.f * z.Value),
                                                                  Vector3(Vector2(x,y), (float32 z.Value)),
                                                                  Vector2(88.f,108.f)
                                                                  );]
                         |> List.map (fun (a, b, v, cv, e) -> ((cv.X, cv.Y, cv.Z), (a, b, v, cv, e)))
                         |> Map.ofList
    let MobileObjects = lazy(MobileLogical
                         |> Map.toList
                         |> List.sortBy (fun ((x, y, z), (_, _, v, cv, _)) -> cv.Z * 1000.f + cv.Y + 0.0001f * cv.X)
                         )


    let AllObjects = lazy(WorldObjects.Value @ MobileObjects.Value
                             |> List.sortBy (fun ((x, y, z), (_, _, v, cv, _)) -> cv.Z / 4.f + cv.Y + cv.X)
                             |> List.map CreateActor')

    let mutable Actors =  Unchecked.defaultof<WorldActor list>

    let mutable Font = Unchecked.defaultof<SpriteFont>
    let mutable FrameRate = 0.0
    let mutable FrameCounter = 0.0

    let DrawActor (sb:SpriteBatch) actor =
        do sb.Draw(actor.Texture, Rectangle(int actor.Position.X, int actor.Position.Y, int actor.Size.X, int actor.Size.Y), Color.White)
        ()

    override x.Initialize() =
        do GameCamera.ViewportWidth <- x.GraphicsDevice.Viewport.Width;
        do GameCamera.ViewportHeight <- x.GraphicsDevice.Viewport.Height;
        do x.Window.AllowUserResizing <- true
        do x.IsMouseVisible <- true
        x.Window.ClientSizeChanged.Add(fun e -> do GameCamera.ViewportWidth <- x.GraphicsDevice.Viewport.Width;
                                                do GameCamera.ViewportHeight <- x.GraphicsDevice.Viewport.Height;
                                                GameCamera.CenterOn(Cell(MapSize/2.f, MapSize/2.f)))
        do spriteBatch <- new SpriteBatch(x.GraphicsDevice)
        
        do base.Initialize()
        ()
 
    override x.LoadContent() =
        do content <- x.Content
        GameCamera.CenterOn(Cell(MapSize/2.f, MapSize/2.f))
        do WorldObjects.Force () |> ignore
        do MobileObjects.Force () |> ignore
        do Actors <- AllObjects.Force ()
        do Font <- content.Load("Zodiac")
        ()
 
    override x.Update (gameTime) =
        if (FrameCounter > 0.0 && gameTime.ElapsedGameTime.TotalSeconds > 0.0) then
            do FrameRate <- FrameCounter / gameTime.ElapsedGameTime.TotalSeconds
            do FrameCounter <- 0.0
        ()
 
    override x.Draw (gameTime) =
        do FrameCounter <- FrameCounter + 1.0;
        do x.GraphicsDevice.Clear Color.CornflowerBlue
        let DrawActor' = DrawActor spriteBatch
        do spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, GameCamera.TranslationMatrix)
        Actors |> List.iter DrawActor'
        //do spriteBatch.DrawString(font, (string (int frameRate)), (camera.Position), Color.OrangeRed)
        do spriteBatch.End ()
        ()