using System;

[AttributeUsage(AttributeTargets.Class)]  // Применяется только к классам
public class GameCharacterAttribute : Attribute
{
    public string Description { get; }

    public GameCharacterAttribute(string description)
    {
        Description = description;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]  // Применяется к свойствам/методам
public class ImportantMemberAttribute : Attribute
{
    public string Remark { get; }

    public ImportantMemberAttribute(string remark)
    {
        Remark = remark;
    }
}