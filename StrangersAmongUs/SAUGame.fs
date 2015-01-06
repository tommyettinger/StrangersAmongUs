module SAUGame
open SAUActor
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
 
type Game1 () as x =
    inherit Game()
 
    do x.Content.RootDirectory <- "Content"
    let graphics = new GraphicsDeviceManager(x)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
 
    let CreateActor' = CreateActor x.Content
        
    let WorldObjects = lazy ([("palette50/Terrain_Huge_face0_0.png", Terrain, Vector2(80.f,0.f), Vector2(168.f,208.f));
                              ("palette0/Human_Male_Large_face0_0.png", Player(Nothing), Vector2(120.f,60.f), Vector2(88.f,108.f));
                              ("", Terrain, Vector2(42.f,60.f), Vector2(32.f,32.f));]
                         |> List.map CreateActor')

    let DrawActor (sb:SpriteBatch) actor =
        if actor.Texture.IsSome then
            do sb.Draw(actor.Texture.Value, actor.Position, Color.White)
        ()

    override x.Initialize() =
        do spriteBatch <- new SpriteBatch(x.GraphicsDevice)
        do base.Initialize()
        ()
 
    override x.LoadContent() =
        do WorldObjects.Force () |> ignore
        ()
 
    override x.Update (gameTime) =
        ()
 
    override x.Draw (gameTime) =
        do x.GraphicsDevice.Clear Color.CornflowerBlue
        let DrawActor' = DrawActor spriteBatch
        do spriteBatch.Begin ()
        WorldObjects.Value
        |> List.iter DrawActor'
        do spriteBatch.End ()
        ()
        