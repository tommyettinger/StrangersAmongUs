module Globals
open MonoGameUtilities
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open System.Collections.Generic

open Utils

let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
let mutable content =  Unchecked.defaultof<ContentManager>
let textures = new Dictionary<(string * int * int * string), Texture2D>(HashIdentity.Structural)
