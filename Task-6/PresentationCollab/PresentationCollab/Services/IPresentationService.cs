using PresentationCollab.Models;

namespace PresentationCollab.Services
{
    public interface IPresentationService
    {
        List<Presentation> GetAllPresentations();
        Presentation GetPresentationById(int id);
        Presentation CreatePresentation(string title, string creatorNickname);
        Slide AddSlide(int presentationId, string content);
        void UpdateSlide(int slideId, string content);
        User UpdateUserRole(int presentationId, string nickname, UserRole role);
        User AddUserToPresentation(int presentationId, string nickname);
    }
}
