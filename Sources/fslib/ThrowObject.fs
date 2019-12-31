namespace FsLib
open UnityEngine

[<AllowNullLiteral>]
type ThrowObjectOfKey() =
    inherit MonoBehaviour()

    let [<SerializeField>] mutable prototype: GameObject = null
    let [<SerializeField>] mutable throwSpeed = 5.f
    let [<SerializeField>] mutable throwInterval = 0.5f
    let [<SerializeField>] mutable throwOffset = Vector3.zero


    let mutable previousThrowTime = 0.f
    member b.Update() =
        if prototype |> isNull |> not && previousThrowTime + throwInterval <= Time.time && Input.GetMouseButton 0 then
            previousThrowTime <- Time.time

            let position = b.transform.position + b.transform.localRotation * throwOffset
            let velocity = throwSpeed * b.transform.forward

            let instance = GameObject.Instantiate<GameObject> prototype
            instance.SetActive true
            instance.transform.position <- position

            let body = instance.GetComponent<Rigidbody>()
            if isNull body then
                Debug.LogWarning "prototype には RigidBody が必要です"
            else
                body.AddForce(velocity, ForceMode.VelocityChange)

    member _.Prototype with get() = prototype and set(v) = prototype <- v

[<RequireComponent(typeof<ThrowObjectOfKey>)>]
type PickupObjectOfKey() as b =
    inherit MonoBehaviour()

    let [<SerializeField>] mutable pickupOffset = Vector3.zero
    let [<SerializeField>] mutable maxDistance = 5.f
    let [<SerializeField>] mutable mouseButton = MouseButton.Right
    let [<SerializeField>] mutable choiceMaterial = null
    let [<SerializeField>] mutable hoverMaterial = null

    let [<SerializeField>] mutable throwObject = null
    let start() =
        throwObject <- b.GetComponent<ThrowObjectOfKey>()

    let raycastHits = Array.zeroCreate 100

    let isPick (hit: RaycastHit byref) =

        // 自分でない
        not <| LanguagePrimitives.PhysicalEquality hit.rigidbody.gameObject b.gameObject &&

        // 地形でない
        let mutable r = null
        not <| hit.rigidbody.gameObject.TryGetComponent<Terrain>(&r)

    let pick() =
        let origin = b.transform.position + b.transform.localRotation * pickupOffset
        let direction = b.transform.forward
        let count = Physics.RaycastNonAlloc(origin, direction, raycastHits, maxDistance)

        let mutable nearest = ValueNone
        for i in 0..count-1 do
            let hit = &raycastHits.[i]
            if isPick &hit then
                match nearest with
                | ValueSome struct(distance, _) when distance <= hit.distance -> ()
                | _ -> nearest <- ValueSome(hit.distance, hit.rigidbody.gameObject)

        nearest

    let restoreObject (ref: _ byref) =
        match ref with
        | ValueNone -> ()
        | ValueSome struct(renderer: Renderer, assignedMaterial, oldMaterial) ->
            // if LanguagePrimitives.PhysicalEquality renderer.material assignedMaterial then
                renderer.material <- oldMaterial
        ref <- ValueNone

    let selectObject (ref: _ byref) (object: GameObject) newMaterial =
        if isNull newMaterial then () else
        let renderer = object.GetComponent<Renderer>()
        if isNull renderer then () else

        let oldMaterial = renderer.material
        renderer.material <- newMaterial
        ref <- ValueSome struct(renderer, newMaterial, oldMaterial)

    let mutable hoverObject = ValueNone
    let restoreHoverObject() = restoreObject &hoverObject
    let selectHoverObject object = selectObject &hoverObject object hoverMaterial

    let mutable choiceObject = ValueNone
    let restoreChoiceObject() = restoreObject &choiceObject
    let selectChoiceObject object = selectObject &choiceObject object choiceMaterial

    let update() =
        restoreChoiceObject()
        restoreHoverObject()

        match pick() with
        | ValueNone -> ()
        | ValueSome(_, object) ->
            selectHoverObject object

            if mouseButton = MouseButton.None || Input.GetMouseButton(int mouseButton) then
                match hoverObject with
                | ValueNone -> ()
                | ValueSome(renderer, _, _) -> 
                    let choiced = renderer.gameObject
                    selectChoiceObject choiced
                    throwObject.Prototype <- choiced

    member _.Start() = start()
    member _.Update() = update()
