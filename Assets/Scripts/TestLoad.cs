using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SUNCGLoader;

public class TestLoad : MonoBehaviour {

    void Start()
    {
        House h = House.LoadFromJson("D:/CS294/suncg_data/house/000d939dc2257995adcb27483b04ad04/house.json");
        Loader l = new Loader();
        l.HouseToScene(h);
    }

}
