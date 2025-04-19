using UnityEngine;

public class Trail
{
    public string Trailname;
    public string distance;
    public float time;
    public int elevation;
    public Location location;

    public Trail(string Trailname, string distance, float time, int elevation, float lat, float lon)
    {
        this.Trailname = Trailname;
        this.distance = distance;
        this.time = time;
        this.elevation = elevation;
        this.location = new Location(lat, lon); 
    }
}
