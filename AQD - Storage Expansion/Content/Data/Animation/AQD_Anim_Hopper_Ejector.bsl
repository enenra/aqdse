@BlockID "AQD_LG_Hopper_Ejector"
@Version 2
@Author enenra

using Door1 as Subpart("Door_1")
using Door2 as Subpart("Door_2")
using Door3 as Subpart("Door_3")
using Door4 as Subpart("Door_4")
using Emitter as Emitter("dummy_detector_ejector_1")

var isOpen = true
var duration = 100

func reset() {
	Door1.Reset()
	Door2.Reset()
	Door3.Reset()
	Door4.Reset()
}

func openDoor() {
    if (isOpen == false) {
        Door1.Rotate([1, 0, 0], -1.0, duration, InQuad)
        Door2.Rotate([1, 0, 0], -1.0, duration, InQuad)
        Door3.Rotate([1, 0, 0], -1.0, duration, InQuad)
        Door4.Rotate([1, 0, 0], -1.0, duration, InQuad)
		API.log("opening")

		Emitter.PlaySound("_GFA_XWing_CargoScoop")
        isOpen = true
    }
}

func closeDoor() {
    if (isOpen == true) {
        Door1.Rotate([1, 0, 0], 1.0, duration, InQuad)
        Door2.Rotate([1, 0, 0], 1.0, duration, InQuad)
        Door3.Rotate([1, 0, 0], 1.0, duration, InQuad)
        Door4.Rotate([1, 0, 0], 1.0, duration, InQuad)
		API.log("closing")

		Emitter.PlaySound("_GFA_XWing_CargoScoop")
        isOpen = false
		API.DelayFunction("reset", duration)
    }
}

action Block() {
	Working() {
		openDoor()
		API.log("working")
	}
	NotWorking() {
		closeDoor()
		API.log("not working")
	}
}