﻿module SAUActor

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
 
type PlayerState =
    | Nothing
 
type ActorType =
    | Player of PlayerState
    | Terrain
 
type WorldActor =
    {
        ActorType : ActorType;
        Position : Vector2;
        Size : Vector2;
        Texture : Texture2D option;
    }
    member this.CurrentBounds
        with get () = Rectangle((int this.Position.X),(int this.Position.Y),(int this.Size.X),(int this.Size.Y))

let CreateActor (content:ContentManager) (textureName, actorType, position, size) =
    let tex = if not (System.String.IsNullOrEmpty textureName) then
                  Some(content.Load textureName)
              else
                  None
    { ActorType = actorType; Position = position; Size = size; Texture = tex; }