namespace FsLib
open UnityEngine

[<RequireComponent(typeof<Rigidbody>)>]
[<RequireComponent(typeof<CharacterController>)>]
type MoveOfKey() =
    inherit MonoBehaviour()

    let [<SerializeField>] mutable moveSpeed = 0.3f
    let [<SerializeField>] mutable maxSpeed = 1.f
    let [<SerializeField>] mutable jumpPower = 3.f

    let mutable body = null
    let mutable characterController = null
    member b.Start() =
        body <- b.GetComponent<Rigidbody>()
        characterController <- b.GetComponent<CharacterController>()

    member b.Update() =
        if body.velocity.magnitude < maxSpeed then
            let d =
                let d = Vector3.zero
                let d = if Input.GetKey KeyCode.W then b.transform.forward else d
                let d = if Input.GetKey KeyCode.S then -b.transform.forward else d
                let d = if Input.GetKey KeyCode.D then b.transform.right else d
                let d = if Input.GetKey KeyCode.A then -b.transform.right else d
                d

            if d <> Vector3.zero then
                body.AddForce(moveSpeed * d, ForceMode.Impulse)

        if characterController.isGrounded then
            body.AddForce(jumpPower * b.transform.up, ForceMode.Impulse)