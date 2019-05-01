using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SUNCGLoader;

public class TestLoad : MonoBehaviour {

    void Start()
    {
        House h = House.LoadFromJson("/Users/gabemontague/Courses/FinalProject/SunCG/house/0a41dad983c48b40618a56cd5772ff97/house.json");
        Loader l = new Loader();
        l.HouseToScene(h);
    }

}
