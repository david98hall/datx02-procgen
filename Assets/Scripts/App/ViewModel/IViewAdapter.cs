
namespace App.ViewModel
{
    internal interface IViewAdapter<T>
    {
        T Model { get; set; }

        void Display();
    }
}