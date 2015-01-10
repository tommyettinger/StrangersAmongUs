module Utils

open System

let R = new Random()
let RandNth s = let n = R.Next() % (Seq.length s)
                s |> Seq.nth n
