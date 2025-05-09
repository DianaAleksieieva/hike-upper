[System.Serializable]
public class Friend
{
    public string id;
}

[System.Serializable]
public class User
{
    public string name;
    public string email;
    public string password;
    public int level;
    public int xp;
    public Friend[] friends;

    public User(string name, string email, string password, int level, int xp, Friend[] friends)
    {
        this.name = name;
        this.email = email;
        this.password = password;
        this.level = level;
        this.xp = xp;
        this.friends = friends;
    }
}
