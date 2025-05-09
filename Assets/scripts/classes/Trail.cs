using UnityEngine;
public class Location
{
    public float latitude;
    public float longitude;

    public Location(float lat, float lon)
    {
        latitude = lat;
        longitude = lon;
    }
}

public class Trail
{
    public string Trailname;
    public string distance;
    public float time;
    public int elevation;
    public Location location;
    public Sprite mapImage;

    // Updated constructor with mapImage as a parameter
    public Trail(string Trailname, string distance, float time, int elevation, float lat, float lon)
    {
        this.Trailname = Trailname;
        this.distance = distance;
        this.time = time;
        this.elevation = elevation;
        this.location = new Location(lat, lon); 

        //load image for trail
        string imagePath = "Maps/" + Trailname.Replace(" ", "");

        mapImage = Resources.Load<Sprite>(imagePath);
    }
}
