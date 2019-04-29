using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SUNCGLoader;

public class TestLoad : MonoBehaviour {

    void Start()
    {
        House h = House.LoadFromJson("/Users/gabemontague/Courses/FinalProject/SunCG/house/00a2a04afad84b16ff330f9038a3d126/house.json");
        Loader l = new Loader();
        l.HouseToScene(h);
    }

}
