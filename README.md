# SUNCGUnityViewer
Tool to view the SUNCG Dataset in Unity http://suncg.cs.princeton.edu/

## To Use:

        House h = House.LoadFromJson("<PATH_TO_HOUSES>/house.json");
        Loader l = new Loader();
        l.HouseToScene(h);
        
You must also set the root path to the SUNCG Data set in Config.cs
