using static OpenNGB.Packlist;

namespace OpenNGB
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PacklistStructure output = Packlist.ReadPacklistStructure("./packlist.dat");
            Console.WriteLine(output.entries[3].name);
            Console.WriteLine(BitConverter.ToString(output.entries[0].key));
        }
    }
}