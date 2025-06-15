namespace Battlegrounds.Helpers;

public delegate void ModalDoneEventHandler(object sender, object result);

public interface INotifyModalDone {

    event ModalDoneEventHandler? ModalDone;

}
