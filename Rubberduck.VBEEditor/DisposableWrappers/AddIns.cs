using System.Collections;

namespace Rubberduck.VBEditor.DisposableWrappers
{
    public class AddIns : SafeComWrapper<Microsoft.Vbe.Interop.Addins>, IEnumerable
    {
        public AddIns(Microsoft.Vbe.Interop.Addins comObject) : 
            base(comObject)
        {
        }

        public AddIn Item(object index)
        {
            return new AddIn(InvokeResult(() => ComObject.Item(index)));
        }

        public void Update()
        {
            Invoke(() => ComObject.Update());
        }

        public VBE VBE { get { return new VBE(InvokeResult(() => ComObject.VBE)); } }
        // returns the host application COM object. todo: figure out how to make it return an IHostApplication
        public object Parent { get { return InvokeResult(() => ComObject.Parent); } } 
        public int Count { get { return InvokeResult(() => ComObject.Count); } }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InvokeResult(() => ComObject.GetEnumerator());
        }
    }
}