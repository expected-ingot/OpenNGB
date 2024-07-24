using static OpenNGB.Packlist;

namespace OpenNGB
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PacklistStructure output = Packlist.ParseFile("./packlist.dat");
            Console.WriteLine(output.entries[3].nameLength);
        }
    }
}