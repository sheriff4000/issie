﻿module Hlp23Tick3

// important file containing all the Draw block (Symbol, BusWire and Sheet) types
// types are arranged in 3 submodules for each of Symbol, BusWire and Sheet
open DrawModelType

// open the submodules for these types which are needed
// so they do not need to be referred to using module name `BusWireT.Wire`
open DrawModelType.SymbolT
open DrawModelType.BusWireT

// open modules possiblly needed for drawSymbol
open Fable.React
open Fable.React.Props
open Elmish

// the standard Issie types for components, connections, etc
open CommonTypes

// some helpers to draw lines, text etc
open DrawHelpers

// the file containing symbol subfunctions etc
open Symbol

/// submodule for constant definitions used in this module
module Constants =
    let house_height = 200
    let house_width = 200 // sample constant definition (with bad name) delete and replace
                  // your constants. Delete this comment as well!

/// Record containing BusWire helper functions that might be needed by updateWireHook
/// functions are fed in at the updatewireHook function call in BusWireUpdate.
/// This is needed because HLPTick3 is earlier in the F# compile order than Buswire so
/// the functions cannot be called directly.
/// Add functions as needed.
/// NB these helpers are not needed to do Tick3
type Tick3BusWireHelpers = {
    AutoRoute: BusWireT.Model -> Wire -> Wire
    ReverseWire: Wire -> Wire
    MoveSegment: Model -> Segment -> float -> Wire
    }


/// Return Some reactElement list to replace drawSymbol by your own code
/// Choose which symbols this function controls by
/// returning None for the default Issie drawSymbol in SymbolView.
/// Drawhelpers contains some helpers you can use to draw lines, circles, etc.
/// drawsymbol contains lots of example code showing how they can be used.
/// The returned value is a list of SVG objects (reactElement list) that will be
/// displayed on screen.
//  for Tick 3 see the Tick 3 Powerpoint for what you need to do.
//  the house picture, and its dependence on the two parameters, will be assessed via interview.
let drawSymbolHook
        (symbol:Symbol) 
        (theme:ThemeType) 
        : ReactElement list option =
    // replace the code below by your own code
    match symbol.Component.Type with
    | Constant1 (windowsH, windowsV, _) when windowsH > 0 && windowsH <=10 && windowsV > 0 && windowsV <= 3 ->
        printfn $"HOUSE: window hori ={windowsH} window vert={windowsV}"
        let width = Constants.house_width
        let height = Constants.house_height
        let halfWinWidth = ((width / windowsH) - 8) / 2 
        let halfWinHeight = ((( height - 35) / int windowsV) - 8) / 3
        
        let makeDoor =
            let middleW = width / 2
            let doorPoints =  sprintf $"{middleW-10},{height},{middleW+10},{height},{middleW+10},{height-25},{middleW-10},{height-25}"
            [makePolygon doorPoints {defaultPolygon with Fill = "No"; FillOpacity = 0.0; Stroke = "Black"; StrokeWidth="2px"}]

        let makeWindow x y =
            let winPoints = sprintf $"{x-halfWinWidth},{y+halfWinHeight},{x+halfWinWidth},{y+halfWinHeight},{x+halfWinWidth},{y-halfWinHeight},{x-halfWinWidth},{y-halfWinHeight}"
            [makePolygon winPoints {defaultPolygon with Fill = "No"; FillOpacity = 0.0; Stroke = "Black"; StrokeWidth="2px"}]

        let makeRow y = 
            let windowPlacer offset winState currWin =
                let multiplierH x = 
                    match x with
                    |0 -> 0
                    |n -> ((2 * n)-1) * ((width/windowsH)/2)
                
                makeWindow (width/2 + (if currWin <> 0 then offset else 0) + (multiplierH currWin)) y @ makeWindow (width/2 - (if currWin <> 0 then offset else 0) - (multiplierH currWin)) y

            match windowsH % 2 with
            | 0 ->
                ([],[1..windowsH/2])
                ||> List.scan (windowPlacer 0)
                |> List.concat
            | 1 ->  
                ([],[0..windowsH/2])
                ||> List.scan (windowPlacer (halfWinWidth + 4))
                |> List.concat

            |_ -> failwithf "error - should not be here"

        let makeAll =
            let centre = (height-25)/2
            let windowStacker offset winState currRow =
                let multiplierV x = 
                    match x with
                    |0 -> 0
                    |n -> ((2 * n)-1) * ((height/int windowsV)/2)

                makeRow (centre + (if currRow <> 0 then offset else 0) + multiplierV currRow ) @ makeRow (centre - (if currRow <> 0 then offset else 0) - multiplierV currRow )

            match int windowsV % 2 with
            | 0 ->
                ([],[1..int windowsV/2])
                ||> List.scan (windowStacker 0)
                |> List.concat
            | 1 ->  
                ([],[0..int windowsV/2])
                ||> List.scan (windowStacker (halfWinHeight + 4))
                |> List.concat
            | _ -> failwithf "error - shouldn't be here"

        let makeHouse =
            let housePoints = sprintf $"0,0,0,{height},{width},{height},{width},0"
            [makePolygon housePoints {defaultPolygon with Fill = "No"; FillOpacity = 0.0; Stroke = "Black"; StrokeWidth="4px"}]
    
        Some (makeHouse @ makeDoor @ makeAll) 

        //Some [makeLine 0 0 10 10 defaultLine]
        // let points = "0,0,0,100,100,100,100,0"
        // Some [makePolygon points {defaultPolygon with Fill = "Black"; FillOpacity = 0.0; Stroke = "DodgerBlue"; StrokeWidth="2px"}] 
        //Some [makeText 0 0 "hello test" defaultText]
        
    | _ -> None

/// Return Some newWire to replace updateWire by your own code defined here.
/// Choose which wires you control by returning None to use the
/// default updateWire function defined in BusWireUpdate.
/// The wire shape and position can be changed by changing wire.Segments and wire.StartPos.
/// See updateWire for the default autoroute wire update function.
/// The return value must be a (possibly modified) copy of wire.

// For tick 3 modify the updated wires (in some cases) somehow. 
// e.g. if they have 3 visual segments and have a standard (you decide what) orientation change where the middle
// segment is on screen so it is 1/3 of the way between the two components instead of 1/2.
// do something more creative or useful if you like.
// This part of Tick will pass if you can demo one wire changing as you move a symbol in some way different from
// Issie: the change need not work on all quadrants (where it is not implemented the wire should default to
// Issie standard.
let updateWireHook 
        (model: BusWireT.Model) 
        (wire: Wire) 
        (tick3Helpers: Tick3BusWireHelpers)
        : Wire option =
    let segmentInfo =
        wire.Segments
        |> List.map (fun (seg:Segment) -> seg.Length,seg.Mode)
    printfn "%s" $"Wire: Initial Orientation={wire.InitialOrientation}\nSegments={segmentInfo}"
    None

//---------------------------------------------------------------------//
//-------included here because it will be used in project work---------//
//---------------------------------------------------------------------//

/// This function is called at the end of a symbol (or multi-symbol) move
/// when the mouse goes up.
/// at this time it would make sense to try for a better autoroute of
/// all the moved wires e.g. avoiding eachother, avoiding other wires,
/// etc, etc.
///
/// wireIds is the list of wire ids that have one end connected to a
/// moved symbol.
/// Any required change in wire positions or shapes should be returned by 
/// changing the values of busWireModel.Wires which
/// is a Map<ConnectionId , Wire> and contains all wires
/// keyed by their wire Id (type ConnectionId)
/// No change required for Tick 3 
let smartAutoRouteWires
        (wireIds: ConnectionId list) 
        (tick3Helpers: Tick3BusWireHelpers)
        (model: SheetT.Model) 
        : SheetT.Model =
    let busWireModel = model.Wire // contained as field of Sheet model
    let symbolModel = model.Wire.Symbol // contained as field of BusWire Model
    let wires = busWireModel.Wires // all wire info
    // NB to return updated wires here you would need nested record update
    // {model with Wire = {model.Wire with Wires = wires'}}
    // Better syntax to do that can be found using optics lenses
    // see DrawModelT for already defined lenses and Issie wiki 
    // for how they work
    model // no smart autoroute for now, so return model with no chnage

//---------------------------------------------------------------------//
//------------------- Snap Functionality-------------------------------//
//---------------------------------------------------------------------//

(*

 Needed for one part of project work (not for Tick 3):
    Sheet.getNewSegmentSnapInfo
    Sheet.getNewSymbolSnapInfo

 These functions can be changed to alter which things symbols or segments snap to:
 They are called at the start of a segment or symbol drag operation.

 If you want to change these inside a drag operation - you may need to alter other code.
 The snap code is all in one place and well-structured - it should be easy to change.

 *)
