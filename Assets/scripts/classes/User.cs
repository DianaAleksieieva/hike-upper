using UnityEngine;

[System.Serializable]
public class User
{
    public string name;
    public int id;
    public User(string name, int id) {
        this.name=name;
        this.id=id;
    }
}
