namespace FsLib
open UnityEngine
open System


[<CLIMutable>]
type KeySet = {
    mutable forward: KeyCode
    mutable back: KeyCode
    mutable right: KeyCode
    mutable left: KeyCode

    mutable jump: KeyCode
}
module KeySet =
    let newDefault() = {
        forward = KeyCode.W
        back = KeyCode.S
        right = KeyCode.D
        left = KeyCode.A

        jump = KeyCode.Space
    }

[<RequireComponent(typeof<Rigidbody>)>]
type MoveOfKey() =
    inherit MonoBehaviour()

    let [<SerializeField>] mutable moveSpeed = 0.3f
    let [<SerializeField>] mutable maxSpeed = 1.f
    let [<SerializeField>] mutable jumpPower = 3.f
    let [<SerializeField>] mutable keySet = KeySet.newDefault()

    let collisionIsGround (b: MoveOfKey) (c: Collision) =
        let mutable isGround = false
        let selfY = b.transform.position.y
        for i in 0..c.contactCount-1 do
            let c = c.GetContact i
            if c.point.y <= selfY then
                isGround <- true
        isGround

    let [<NonSerialized>] mutable isGround = false
    let onCollisionStay (b: MoveOfKey) (c: Collision) =
        if collisionIsGround b c then
            isGround <- true

    let [<NonSerialized>] mutable body = null
    member b.Start() =
        body <- b.GetComponent<Rigidbody>()

    member _.FixedUpdate() =
        isGround <- false

    member b.OnCollisionStay(c: Collision) = onCollisionStay b c

    member b.Update() =
        if body.velocity.magnitude < maxSpeed then
            let d =
                let d = Vector3.zero
                let d = if Input.GetKey keySet.forward then b.transform.forward else d
                let d = if Input.GetKey keySet.back then -b.transform.forward else d
                let d = if Input.GetKey keySet.right then b.transform.right else d
                let d = if Input.GetKey keySet.left then -b.transform.right else d
                d

            if d <> Vector3.zero then
                body.AddForce(moveSpeed * d, ForceMode.Impulse)

        if isGround && Input.GetKey keySet.jump then
            body.AddForce(jumpPower * b.transform.up, ForceMode.Impulse)
