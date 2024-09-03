using FluentAssertions;

namespace ExtensionsTesting;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Empty()
    {
        List<Tes> left = [];
        List<Tes> right = [];
        IEnumerable<Tes> res =  left.FullOuterJoin(right, l => l.Id, r => r.Id, (left, right) => right).ToList();
        res.Should().HaveCount(0);
    }
    
    [Test]
    public void LeftOnly()
    {
        List<Tes> left = [new Tes(1), new Tes(2)];
        List<Tes> right = [];
        IEnumerable<(Tes? LEFT, Tes? RIGHT)> res =  left.FullOuterJoin(right, l => l.Id, r => r.Id, (left, right) => (left, right)).ToList();
        Console.WriteLine(string.Join(' ', res));
        res.Should().HaveCount(2);
        res.Should().ContainSingle(tup => tup.LEFT!.Id == 1 && tup.RIGHT == null);
        res.Should().ContainSingle(tup => tup.LEFT!.Id == 2 && tup.RIGHT == null);
    }
    
    [Test]
    public void RightOnly()
    {
        List<Tes> left = [];
        List<Tes> right = [new Tes(1), new Tes(2)];
        IEnumerable<(Tes? LEFT, Tes? RIGHT)> res =  left.FullOuterJoin(right, l => l.Id, r => r.Id, (left, right) => (left, right)).ToList();
        Console.WriteLine(string.Join(' ', res));
        res.Should().HaveCount(2);
        res.Should().ContainSingle(tup => tup.LEFT == null && tup.RIGHT!.Id == 1);
        res.Should().ContainSingle(tup => tup.LEFT == null && tup.RIGHT!.Id == 2);
    }
    
    [Test]
    public void Mix()
    {
        List<Tes> left = [new Tes(1), new Tes(2)];
        List<Tes> right = [new Tes(2), new Tes(3)];
        IEnumerable<(Tes? LEFT, Tes? RIGHT)> res =  left.FullOuterJoin(right, l => l.Id, r => r.Id, (left, right) => (left, right)).ToList();
        IEnumerable<(Tes? LEFT, Tes? RIGHT)> sanity =  left.SelectMany(left => right, (left, right) => (left, right));
        Console.WriteLine(string.Join(' ', res));
        Console.WriteLine(string.Join(' ', sanity));
        res.Should().HaveCount(3);
        res.Should().ContainSingle(tup => tup.LEFT != null && tup.LEFT!.Id == 1 && tup.RIGHT == null);
        res.Should().ContainSingle(tup => tup.LEFT != null  && tup.LEFT.Id == 2 && tup.RIGHT != null && tup.RIGHT.Id == 2);
        res.Should().ContainSingle(tup => tup.LEFT == null && tup.RIGHT != null && tup.RIGHT!.Id == 3);
    }
    
    [Test]
    public void Mix2()
    {
        List<Tes> left = [new Tes(2), new Tes(2), new Tes(1)];
        List<Tes> right = [new Tes(2), new Tes(3), new Tes(2), new Tes(2)];
        IEnumerable<(Tes? LEFT, Tes? RIGHT)> res =  left.FullOuterJoinDistinct(right, l => l.Id, r => r.Id, (left, right) => (left, right)).ToList();
        Console.WriteLine(string.Join(' ', res));
        res.Should().HaveCount(3);
        // res.Should().ContainSingle(tup => tup.LEFT != null && tup.LEFT!.Id == 1 && tup.RIGHT == null);
        // res.Should().ContainSingle(tup => tup.LEFT != null  && tup.LEFT.Id == 2 && tup.RIGHT != null && tup.RIGHT.Id == 2);
        // res.Should().ContainSingle(tup => tup.LEFT != null  && tup.LEFT.Id == 2 && tup.RIGHT != null && tup.RIGHT.Id == 2);
        // res.Should().ContainSingle(tup => tup.LEFT != null  && tup.LEFT.Id == 2 && tup.RIGHT != null && tup.RIGHT.Id == 2);
        // res.Should().ContainSingle(tup => tup.LEFT == null && tup.RIGHT != null && tup.RIGHT!.Id == 3);
    }
    
    
    [Test]
    public void Mix_Reversed_ReturnsReverseTuples()
    {
        List<Tes> left = [new Tes(1), new Tes(2)];
        List<Tes> right = [new Tes(2), new Tes(3)];
        IEnumerable<(Tes? LEFT, Tes? RIGHT)> res =  left.FullOuterJoin(right, l => l.Id, r => r.Id, (left, right) => (left, right)).ToList();
        Console.WriteLine(string.Join(' ', res));
        res.Should().ContainSingle(tup => tup.LEFT != null && tup.LEFT!.Id == 1 && tup.RIGHT == null);
        res.Should().ContainSingle(tup => tup.LEFT != null  && tup.LEFT.Id == 2 && tup.RIGHT != null && tup.RIGHT.Id == 2);
        res.Should().ContainSingle(tup => tup.LEFT == null && tup.RIGHT != null && tup.RIGHT!.Id == 3);
    }
    

    private record Tes
    {
        public Tes(int id)
        {
            Id = id;
        }
        
        public int Id { get; set; }

        public static bool IsEqual(Tes? one, Tes? two)
        {
            if (one is null || two is null) return false;
            return one.Id == two.Id;
        }
    }
}