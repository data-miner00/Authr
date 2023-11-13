namespace Authr.WebApi;

using System.Text.Json.Serialization;

public class Person : IEquatable<Person>
{
    public Person(string firstName, string lastName, DateTime dateOfBirth, int height)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Height = height;
    }

    [JsonPropertyName("first_name")]
    public string FirstName { get; init; }

    [JsonPropertyName("last_name")]
    public string LastName { get; init; }

    [JsonPropertyName("dob")]
    public DateTime DateOfBirth { get; init; }

    [JsonPropertyName("height")]
    public int Height { get; init; }

    public override string ToString()
    {
        return $"Person {{ FirstName = {FirstName}, LastName = {LastName}, DateOfBirth = {DateOfBirth}, Height = {Height} }}";
    }

    public static bool operator !=(Person left, Person right)
    {
        return !(left == right);
    }

    public static bool operator ==(Person left, Person right)
    {
        if ((object)left != right)
        {
            if ((object)left != null)
            {
                return left.Equals(right);
            }
            return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        return int.MaxValue;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Person);
    }

    public virtual bool Equals(Person other)
    {
        return Equals(other);
    }

    public void Deconstruct(out string FirstName, out string LastName, out DateTime DateOfBirth, out int Height)
    {
        FirstName = this.FirstName;
        LastName = this.LastName;
        DateOfBirth = this.DateOfBirth;
        Height = this.Height;
    }
}

public record Person2(
    [property: JsonPropertyName("first_name")] string FirstName,
    [property: JsonPropertyName("last_name")]  string LastName,
    [property: JsonPropertyName("dob")]        DateTime DateOfBirth,
    [property: JsonPropertyName("height")]     int Height);
