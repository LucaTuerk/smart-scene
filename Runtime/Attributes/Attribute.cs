using System;
// Attribute base class
public class Attribute {
    protected String name;
    public String Name {
        get {
            return name;
        }
    }
}

public class BooleanAttribute : Attribute {
    bool value;
    public bool Value {
        get {
            return value;
        }
    }

    public BooleanAttribute ( String name, bool value ) {
        this.name = name;
        this.value = value;
    }
}

