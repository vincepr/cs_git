using System.Diagnostics;
using NUnit.Framework;

namespace DoubleCola_BrainTeaser;

public class BrainTeaser
{
    // should this be here, probably not, but it was fun to do so here we go.
    
    // Sheldon, Leonard, Penny, Rajesh and Howard are in the queue for a "Double Cola" drink vending machine; there are
    // no other people in the queue. The first one in the queue (Sheldon) buys a can, drinks it and doubles!
    // The resulting two Sheldons go to the end of the queue. Then the next in the queue (Leonard) buys a can, drinks it
    // and gets to the end of the queue as two Leonards, and so on. This process continues ad infinitum.
    //
    //     For example, Penny drinks the third can of cola and the queue will look like this:
    //              Rajesh, Howard, Sheldon, Sheldon, Leonard, Leonard, Penny, Penny.
    //
    //     Write a program that will print the name of a man who will drink the n-th can.
    //
    //     Note that in the very beginning the queue looks like that: Sheldon, Leonard, Penny, Rajesh, Howard.
    //              The first person is Sheldon.
    //
    //     Input
    //     The input data consist of a single integer n.
    //
    // It is guaranteed that the pretests check the spelling of all the five names, that is, that they contain all the
    //      five possible answers.
    //
    //     Output
    //     Print the single line — the name of the person who drinks the n-th can of cola.

    [Test]
    public void FirstIteration()
    {
        Assert.That(DoubleCola.RunProblem(1), Is.EqualTo(P.Sheldon));
        Assert.That(DoubleCola.RunProblem(2), Is.EqualTo(P.Leonard));
        Assert.That(DoubleCola.RunProblem(3), Is.EqualTo(P.Penny));
        Assert.That(DoubleCola.RunProblem(4), Is.EqualTo(P.Rajesh));
        Assert.That(DoubleCola.RunProblem(5), Is.EqualTo(P.Howard));
    }
    
    [Test]
    public void SecondIteration()
    {
        Assert.That(DoubleCola.RunProblem(6), Is.EqualTo(P.Sheldon));
        Assert.That(DoubleCola.RunProblem(7), Is.EqualTo(P.Sheldon));
        Assert.That(DoubleCola.RunProblem(8), Is.EqualTo(P.Leonard));
        Assert.That(DoubleCola.RunProblem(9), Is.EqualTo(P.Leonard));
        Assert.That(DoubleCola.RunProblem(10), Is.EqualTo(P.Penny));
        Assert.That(DoubleCola.RunProblem(11), Is.EqualTo(P.Penny));
        Assert.That(DoubleCola.RunProblem(12), Is.EqualTo(P.Rajesh));
        Assert.That(DoubleCola.RunProblem(13), Is.EqualTo(P.Rajesh));
        Assert.That(DoubleCola.RunProblem(14), Is.EqualTo(P.Howard));
        Assert.That(DoubleCola.RunProblem(15), Is.EqualTo(P.Howard));
    }
    
    [Test]
    public void ThirdIteration()
    {
        Assert.That(DoubleCola.RunProblem(16), Is.EqualTo(P.Sheldon));
        Assert.That(DoubleCola.RunProblem(17), Is.EqualTo(P.Sheldon));
        Assert.That(DoubleCola.RunProblem(18), Is.EqualTo(P.Sheldon));
        Assert.That(DoubleCola.RunProblem(19), Is.EqualTo(P.Sheldon));
        Assert.That(DoubleCola.RunProblem(20), Is.EqualTo(P.Leonard));
        Assert.That(DoubleCola.RunProblem(21), Is.EqualTo(P.Leonard));
        Assert.That(DoubleCola.RunProblem(22), Is.EqualTo(P.Leonard));
        Assert.That(DoubleCola.RunProblem(23), Is.EqualTo(P.Leonard));
        Assert.That(DoubleCola.RunProblem(24), Is.EqualTo(P.Penny));
        Assert.That(DoubleCola.RunProblem(25), Is.EqualTo(P.Penny));
        Assert.That(DoubleCola.RunProblem(26), Is.EqualTo(P.Penny));
        Assert.That(DoubleCola.RunProblem(27), Is.EqualTo(P.Penny));
        Assert.That(DoubleCola.RunProblem(28), Is.EqualTo(P.Rajesh));
        Assert.That(DoubleCola.RunProblem(29), Is.EqualTo(P.Rajesh));
        Assert.That(DoubleCola.RunProblem(30), Is.EqualTo(P.Rajesh));
        Assert.That(DoubleCola.RunProblem(31), Is.EqualTo(P.Rajesh));
        Assert.That(DoubleCola.RunProblem(32), Is.EqualTo(P.Howard));
        Assert.That(DoubleCola.RunProblem(33), Is.EqualTo(P.Howard));
        Assert.That(DoubleCola.RunProblem(34), Is.EqualTo(P.Howard));
        Assert.That(DoubleCola.RunProblem(35), Is.EqualTo(P.Howard));
    }
    
    [Test]
    public void Test1802()
    {
        Assert.That(DoubleCola.RunProblem(1802), Is.EqualTo(P.Penny));
    }
}

public class DoubleCola
{
    private readonly IReadOnlyList<P> _positionsList;

    private DoubleCola(List<P> persons)
    {
        _positionsList = persons.AsReadOnly();
    }

    private P Nth(int i)
    {
        var len = _positionsList.Count;
        long sumSoFar = 0;
        for (var iter = 0; iter < int.MaxValue; iter++)
        {
            var iterationsThisLoop = Convert.ToInt64(Math.Pow(2, iter));
            if (i <= sumSoFar + iterationsThisLoop * len)
            {
                for (var j = 0; j < len; j++)
                {
                    if (i <= sumSoFar + (j+1) * iterationsThisLoop) 
                        return _positionsList[j];
                }
            }
            sumSoFar += iterationsThisLoop * len;
        }

        throw new UnreachableException("ran out of bounds. Float power -> long will crash long before.");
    }

    internal static P RunProblem(int i)
    {
        List<P> list = [P.Sheldon, P.Leonard, P.Penny, P.Rajesh, P.Howard];
        var self = new DoubleCola(list);
        return self.Nth(i);
    }
}

internal enum P
{
    Sheldon,
    Leonard,
    Penny,
    Rajesh,
    Howard,
}
