namespace LOIM.Util;

public static class QuestionAnswerValidationUtils
{
    public static bool ValidateAnswer(this ReadOnlySpan<char> answer, byte length, char rangeStart = 'A',
                                      byte                    rangeLength = 4) => answer.Length == length &&
                                                                                  !answer
                                                                                     .ContainsAnyExceptInRange(rangeStart,
                                                                                                               (char)(
                                                                                                                   rangeStart +
                                                                                                                   rangeLength - 1));
}
