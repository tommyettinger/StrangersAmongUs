module SAUActor
open System.IO
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open Globals

type PlayerState =
    | Nothing
 
type ActorType =
    | Player of PlayerState
    | Terrain
 
type WorldActor =
    {
        ActorType : ActorType;
        Position : Vector2;
        GridSpace : Vector3;
        Size : Vector2;
        Texture : Texture2D;
    }
    member this.CurrentBounds
        with get () = Rectangle((int this.Position.X),(int this.Position.Y),(int this.Size.X),(int this.Size.Y))

let voxels = lazy(content.Load<Texture2D> "voxels")
let byteMasks = [|1; 2; 4; 8; 16; 32; 64; 128;|] |> Array.map byte
let RenderActor (device:GraphicsDevice) (k, ((unitName, (cellSize : int), palette, facingString), actorType, position, gridSpace, (size : Vector2))) =
    let dwidth = device.DisplayMode.Width
    let dheight = device.DisplayMode.Height
    let renderTarget = new RenderTarget2D(device, (int size.X * 2), (int size.Y * 2))
    do device.SetRenderTarget(renderTarget);
    let culledBytes = File.ReadAllBytes("Content/" + unitName + "_[" + (string cellSize) + "]_" + facingString + ".tvx")
    let vv = voxels.Value
    do spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null)
    do device.Clear(Color.Transparent)
    for segi = 0 to (culledBytes.Length / 10 - 1) do
        let segment = Array.sub culledBytes (segi * 10) 4
        let [| x; y; z; color; |] = segment
        do spriteBatch.Draw(vv, Rectangle((int (x + y)) * 2 + 2, (int cellSize) * 4 - 2 + ((int x) - (int y) - (int z) * 3), 4, 4), Nullable(Rectangle(4 * (int color), 5 * palette, 4, 4)), Color.White)
        ()
    for segi = (culledBytes.Length / 10 - 1) downto 0 do
        let segment = Array.sub culledBytes (segi * 10) 10
        let [| x; y; z; color; b0; b1; b2; b3; b4; b5 |] = segment
        let bits = [| b0; b1; b2; b3; b4; b5 |]
        for row in [0; 1; 4; 5;] do
            for col in 0 .. 7 do
                if (bits.[row] &&& byteMasks.[col]) <> (byte 0) then do
                   spriteBatch.Draw(vv,
                                    Rectangle((int (x + y)) * 2 + col, (int cellSize) * 4 - 4 + row + ((int x) - (int y) - (int z) * 3), 1, 1),
                                    Nullable(Rectangle(4 * (int color), 5 * palette + 4, 1, 1)),
                                    Color.White)
        for row in [2; 3;] do
            for col in [0; 1; 4; 5;] do
                if (bits.[row] &&& byteMasks.[col]) <> (byte 0) then do
                   spriteBatch.Draw(vv,
                                    Rectangle((int (x + y)) * 2 + (col % 2), (int cellSize) * 4 - 2 + (row - 2) * 2 + ((int x) - (int y) - (int z) * 3), 1, 1),
                                    Nullable(Rectangle(4 * (int color), 5 * palette + 4, 1, 1)),
                                    Color.White)
            for col in [2; 3; 6; 7] do
                if (bits.[row] &&& byteMasks.[col]) <> (byte 0) then do
                   spriteBatch.Draw(vv,
                                    Rectangle((int (x + y)) * 2 + 6 + (col % 2), (int cellSize) * 4 - 2 + (row - 2) * 2 + ((int x) - (int y) - (int z) * 3), 1, 1),
                                    Nullable(Rectangle(4 * (int color), 5 * palette + 4, 1, 1)),
                                    Color.White)
        ()
    
    do spriteBatch.End()
    device.SetRenderTarget(null);
    renderTarget :> Texture2D

let CreateActor (device:GraphicsDevice) (k, (display, actorType, position, gridSpace, (size : Vector2))) =
    let tex = if textures.ContainsKey(display) then textures.[display]
              else let t = RenderActor(device) (k, (display, actorType, position, gridSpace, size))
                   textures.[display] <- t
                   t
    { ActorType = actorType; Position = position; GridSpace = gridSpace; Size = size; Texture = tex; }