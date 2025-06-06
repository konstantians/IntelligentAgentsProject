namespace IntelligentAgents.AuthLibrary.Models.ResponseModels;

public enum LibraryReturnedCodes
{
    UserNotFoundWithGivenId,
    UserNotFoundWithGivenEmail,
    UserAccountLocked,
    UserAccountNotActivated,
    InvalidCredentials,
    ValidTokenButUserNotInSystem,
    InvalidToken,
    UnknownError,
    NoError
}
