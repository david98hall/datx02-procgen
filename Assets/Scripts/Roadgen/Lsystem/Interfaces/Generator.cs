namespace Interfaces
{
    public class Generator<T> : IGenerator<T> where T : class
    {
        public IGenerator<T> Strategy { get; set; }

        public T Generate() => Strategy?.Generate();
    }
}