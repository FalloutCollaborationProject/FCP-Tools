using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace FCP.Core.Buildings
{
    public class Window_TerminalHacking : Window
    {
        private const int LINES_PER_COLUMN = 17;
        private const int CHARS_PER_LINE = 20;
        private const string SYMBOLS = "!@#$%^&*()[]{}/<>|\\~`=+-_";

        private readonly CompTerminalHacking comp;
        private readonly List<string> leftColumn = new List<string>();
        private readonly List<string> rightColumn = new List<string>();
        private readonly List<HackableElement> hackableElements = new List<HackableElement>();
        private readonly List<string> attemptHistory = new List<string>();
        private readonly int baseAddress = 0xF000;

        public override Vector2 InitialSize => new Vector2(900f, 700f);

        public Window_TerminalHacking(CompTerminalHacking comp)
        {
            this.comp = comp;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            GenerateDisplay();
        }

        private void GenerateDisplay()
        {
            leftColumn.Clear();
            rightColumn.Clear();
            hackableElements.Clear();

            var words = comp.DisplayedWords.ToList();
            
            if (!words.Contains(comp.CorrectPassword))
            {
                words.Insert(0, comp.CorrectPassword);
            }
            
            int halfCount = words.Count / 2;
            var leftWords = new List<string>(halfCount);
            var rightWords = new List<string>(words.Count - halfCount);
            
            for (int i = 0; i < halfCount; i++)
                leftWords.Add(words[i]);
            for (int i = halfCount; i < words.Count; i++)
                rightWords.Add(words[i]);

            GenerateColumn(leftColumn, leftWords, 0);
            GenerateColumn(rightColumn, rightWords, LINES_PER_COLUMN);
        }

        private void GenerateColumn(List<string> column, List<string> wordsToPlace, int startLine)
        {
            for (int line = 0; line < LINES_PER_COLUMN; line++)
            {
                column.Add(GenerateGibberishLine());
            }

            foreach (var word in wordsToPlace)
            {
                bool placed = false;
                
                for (int attempt = 0; attempt < 200 && !placed; attempt++)
                {
                    int line = Rand.Range(0, LINES_PER_COLUMN);
                    int pos = Rand.Range(0, CHARS_PER_LINE - word.Length);

                    if (CanPlaceWord(column, line, pos, word.Length))
                    {
                        string lineText = column[line];
                        column[line] = lineText.Substring(0, pos) + word + lineText.Substring(pos + word.Length);
                        
                        hackableElements.Add(new HackableElement
                        {
                            type = HackType.Word,
                            text = word,
                            line = startLine + line,
                            startPos = pos,
                            endPos = pos + word.Length
                        });
                        
                        placed = true;
                    }
                }
                
                if (!placed)
                {
                    for (int line = 0; line < LINES_PER_COLUMN && !placed; line++)
                    {
                        for (int pos = 0; pos <= CHARS_PER_LINE - word.Length; pos++)
                        {
                            if (CanPlaceWord(column, line, pos, word.Length))
                            {
                                string lineText = column[line];
                                column[line] = lineText.Substring(0, pos) + word + lineText.Substring(pos + word.Length);
                                
                                hackableElements.Add(new HackableElement
                                {
                                    type = HackType.Word,
                                    text = word,
                                    line = startLine + line,
                                    startPos = pos,
                                    endPos = pos + word.Length
                                });
                                
                                placed = true;
                                break;
                            }
                        }
                    }
                }
            }

            GenerateBracketPairs(column, startLine);
        }

        private bool CanPlaceWord(List<string> column, int line, int pos, int length)
        {
            if (pos + length > CHARS_PER_LINE) 
                return false;
            
            string lineText = column[line];
            for (int i = pos; i < pos + length; i++)
            {
                if (!SYMBOLS.Contains(lineText[i]))
                    return false;
            }
            return true;
        }

        private void GenerateBracketPairs(List<string> column, int startLine)
        {
            var brackets = new[] { ('(', ')'), ('[', ']'), ('{', '}'), ('<', '>') };
            int pairsToAdd = Rand.Range(3, 6);

            for (int i = 0; i < pairsToAdd; i++)
            {
                var bracket = brackets[Rand.Range(0, brackets.Length)];
                
                for (int attempt = 0; attempt < 50; attempt++)
                {
                    int line = Rand.Range(0, LINES_PER_COLUMN);
                    string lineText = column[line];
                    
                    int openPos = -1;
                    for (int j = 0; j < lineText.Length; j++)
                    {
                        if (lineText[j] == bracket.Item1)
                        {
                            openPos = j;
                            break;
                        }
                    }

                    if (openPos >= 0)
                    {
                        for (int j = openPos + 1; j < lineText.Length; j++)
                        {
                            if (lineText[j] == bracket.Item2)
                            {
                                bool allSymbols = true;
                                for (int k = openPos; k <= j; k++)
                                {
                                    if (!SYMBOLS.Contains(lineText[k]))
                                    {
                                        allSymbols = false;
                                        break;
                                    }
                                }
                                
                                if (allSymbols)
                                {
                                    hackableElements.Add(new HackableElement
                                    {
                                        type = HackType.BracketPair,
                                        text = lineText.Substring(openPos, j - openPos + 1),
                                        line = startLine + line,
                                        startPos = openPos,
                                        endPos = j + 1
                                    });
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (hackableElements.Count > i + GetWordCount())
                        break;
                }
            }
        }

        private int GetWordCount()
        {
            return hackableElements.Count(e => e.type == HackType.Word);
        }

        private string GenerateGibberishLine()
        {
            char[] chars = new char[CHARS_PER_LINE];
            for (int i = 0; i < CHARS_PER_LINE; i++)
            {
                chars[i] = SYMBOLS[Rand.Range(0, SYMBOLS.Length)];
            }
            return new string(chars);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Tiny;
            
            float columnWidth = 280f;
            float spacing = 20f;
            float feedbackWidth = 250f;
            
            Rect leftRect = new Rect(inRect.x, inRect.y + 50f, columnWidth, inRect.height - 100f);
            Rect rightRect = new Rect(leftRect.xMax + spacing, leftRect.y, columnWidth, leftRect.height);
            Rect feedbackRect = new Rect(rightRect.xMax + spacing, leftRect.y, feedbackWidth, leftRect.height);

            DrawHeader(new Rect(inRect.x, inRect.y, inRect.width, 40f));
            DrawColumn(leftRect, leftColumn, 0);
            DrawColumn(rightRect, rightColumn, LINES_PER_COLUMN);
            DrawFeedback(feedbackRect);
        }

        private void DrawHeader(Rect rect)
        {
            Color oldColor = GUI.color;
            GUI.color = TerminalColors.PrimaryColor;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, "ROBCO INDUSTRIES (TM) TERMLINK PROTOCOL");
            Text.Font = GameFont.Small;
            
            Rect attemptsRect = new Rect(rect.x, rect.y + 30f, rect.width, 30f);
            int filled = comp.AttemptsRemaining;
            int empty = comp.Props.maxAttempts - filled;
            
            string attemptsText = "";
            for (int i = 0; i < filled; i++)
                attemptsText += "█ ";
            for (int i = 0; i < empty; i++)
                attemptsText += "░ ";
            
            Widgets.Label(attemptsRect, attemptsText);
            GUI.color = oldColor;
        }

        private void DrawColumn(Rect rect, List<string> column, int startLine)
        {
            Color oldColor = GUI.color;
            GUI.color = TerminalColors.PrimaryColor;
            float lineHeight = 20f;
            int address = baseAddress + (startLine * 12);

            for (int i = 0; i < column.Count; i++)
            {
                Rect lineRect = new Rect(rect.x, rect.y + i * lineHeight, rect.width, lineHeight);
                string addressText = $"0x{address:X4}";
                
                Rect addressRect = new Rect(lineRect.x, lineRect.y, 60f, lineHeight);
                Rect textRect = new Rect(lineRect.x + 65f, lineRect.y, lineRect.width - 65f, lineHeight);
                
                Widgets.Label(addressRect, addressText);
                DrawInteractiveLine(textRect, column[i], startLine + i);
                
                address += 12;
            }
            GUI.color = oldColor;
        }

        private void DrawInteractiveLine(Rect rect, string lineText, int lineIndex)
        {
            Text.Font = GameFont.Tiny;
            float charWidth = Text.CalcSize("A").x;
            
            for (int i = 0; i < hackableElements.Count; i++)
            {
                var element = hackableElements[i];
                if (element.line != lineIndex)
                    continue;

                Rect elementRect = new Rect(
                    rect.x + element.startPos * charWidth, 
                    rect.y, 
                    element.text.Length * charWidth, 
                    rect.height);
                
                if (Mouse.IsOver(elementRect))
                {
                    Widgets.DrawHighlight(elementRect);
                }
                
                if (Widgets.ButtonInvisible(elementRect))
                {
                    OnElementClicked(element);
                }
            }
            
            Widgets.Label(rect, lineText);
        }

        private void DrawFeedback(Rect rect)
        {
            Color oldColor = GUI.color;
            GUI.color = TerminalColors.PrimaryColor;
            Text.Font = GameFont.Tiny;
            float y = rect.y;
            float lineHeight = 18f;

            int count = attemptHistory.Count;
            int startIndex = count > 15 ? count - 15 : 0;
            
            for (int i = count - 1; i >= startIndex; i--)
            {
                Rect entryRect = new Rect(rect.x, y, rect.width, lineHeight);
                Widgets.Label(entryRect, attemptHistory[i]);
                y += lineHeight;
            }
            GUI.color = oldColor;
        }

        private void OnElementClicked(HackableElement element)
        {
            if (element.type == HackType.Word)
            {
                if (element.text.Length > 0 && element.text[0] == '.')
                    return;
                
                int likeness = comp.CheckLikeness(element.text);
                attemptHistory.Add($">{element.text}");
                attemptHistory.Add($">Likeness={likeness}");
                
                if (comp.TryHack(element.text))
                {
                    attemptHistory.Add(">ACCESS GRANTED");
                    Messages.Message("FCP_TerminalHackSuccess".Translate(), MessageTypeDefOf.PositiveEvent);
                    Close();
                    return;
                }
                
                hackableElements.Remove(element);
                RegenerateColumnForElement(element);
                
                if (comp.AttemptsRemaining <= 0)
                {
                    attemptHistory.Add(">ACCESS DENIED");
                    attemptHistory.Add($">LOCKOUT: {comp.Props.lockoutDurationHours}H");
                    Messages.Message("FCP_TerminalLockedOut".Translate(comp.Props.lockoutDurationHours), MessageTypeDefOf.NegativeEvent);
                    Close();
                }
            }
            else if (element.type == HackType.BracketPair)
            {
                if (Rand.Bool)
                {
                    comp.RestoreAttempt();
                    attemptHistory.Add(">ATTEMPTS RESET");
                }
                else
                {
                    for (int i = 0; i < comp.DisplayedWords.Count; i++)
                    {
                        string word = comp.DisplayedWords[i];
                        if (word != comp.CorrectPassword && word.Length > 0 && word[0] != '.')
                        {
                            comp.RemoveDud(word);
                            for (int j = hackableElements.Count - 1; j >= 0; j--)
                            {
                                if (hackableElements[j].type == HackType.Word && hackableElements[j].text == word)
                                {
                                    RegenerateColumnForElement(hackableElements[j]);
                                    hackableElements.RemoveAt(j);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    attemptHistory.Add(">DUD REMOVED");
                }
                
                hackableElements.Remove(element);
                RegenerateColumnForElement(element);
            }
        }

        private void RegenerateColumnForElement(HackableElement element)
        {
            string dots = new string('.', element.endPos - element.startPos);
            
            if (element.line < LINES_PER_COLUMN)
            {
                leftColumn[element.line] = leftColumn[element.line].Substring(0, element.startPos) + 
                                           dots + 
                                           leftColumn[element.line].Substring(element.endPos);
            }
            else
            {
                int lineIndex = element.line - LINES_PER_COLUMN;
                rightColumn[lineIndex] = rightColumn[lineIndex].Substring(0, element.startPos) + 
                                        dots + 
                                        rightColumn[lineIndex].Substring(element.endPos);
            }
        }

        private class HackableElement
        {
            public HackType type;
            public string text;
            public int line;
            public int startPos;
            public int endPos;
        }

        private enum HackType
        {
            Word,
            BracketPair
        }
    }
}
