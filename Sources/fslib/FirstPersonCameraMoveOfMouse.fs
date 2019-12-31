namespace FsLib
open UnityEngine
open System

type MouseButton =
    | None = -1
    | Left = 0
    | Right = 1

type FirstParsonCameraMoveOfMouse() =
    inherit MonoBehaviour()

    let [<SerializeField>] mutable mouseButton = MouseButton.Left
    let [<SerializeField>] mutable rotationSpeed = Vector3.one
    let [<SerializeField>] mutable xTransform: Transform = null
    let [<SerializeField>] mutable yTransform: Transform = null

    let mutable cameraInChild = null
    let start (b: FirstParsonCameraMoveOfMouse) =
         cameraInChild <- b.GetComponentInChildren<Camera>()

    let mutable newAngle = Vector3.zero
    let mutable lastMousePosition = Vector3.zero
    let onMouseDown() =
        lastMousePosition <- Input.mousePosition

    let onMousePush() =
        newAngle.y <- newAngle.y - (lastMousePosition.x - Input.mousePosition.x) * rotationSpeed.y
        newAngle.x <- newAngle.x - (Input.mousePosition.y - lastMousePosition.y) * rotationSpeed.x
        lastMousePosition <- Input.mousePosition

        if isNull yTransform |> not then
            yTransform.eulerAngles <-
                let mutable a = yTransform.eulerAngles
                a.y <- newAngle.y
                a

        if isNull xTransform |> not then
            xTransform.eulerAngles <-
                let mutable a = xTransform.eulerAngles
                a.x <- newAngle.x
                a

    member b.Start() = start b
    member _.Update() =
        if Input.GetMouseButtonDown(int mouseButton) then
            onMouseDown()

        if mouseButton = MouseButton.None || Input.GetMouseButton(int mouseButton) then
            onMousePush()
