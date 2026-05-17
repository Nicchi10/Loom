using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Loom.Core.Enums;
using Loom.Core.Interfaces;
using Loom.Core.Models;

namespace Loom.Engine.Assemblers
{
    public class PromptAssembler : IAssembler
    {
        /// <summary>
        /// Entry point: transform invocation into a messages list
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns> List of all messages </returns>
        public List<Message> Assemble(LlmInvocation invocation)
        {

            // Copy of original message list
            var finalMessages = new List<Message>(invocation.Conversation.Messages);

            var contextBlock = BuildContextBlock(invocation);

            // Finds system message or creates new one on top
            var systemMsg = finalMessages.FirstOrDefault(m => m.Role == MessageRole.System);

            if (systemMsg != null)
            {
                systemMsg.Content = AddContent(systemMsg.Content, contextBlock);
            } else
            {
                finalMessages.Insert(0, new Message
                {
                    Role = MessageRole.System,
                    Content = AddContent(invocation.Conversation.SystemPrompt, contextBlock)
                });
            }

            return finalMessages;
        }

        /// <summary>
        /// Assembles each role kind with its content (user/tool/assistant)
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns> Object with planned structure </returns>
        public List<AssembledItem> AssembleInputItems(LlmInvocation invocation)
        {

            var msgs = Assemble(invocation);
            var items = new List<AssembledItem>();

            foreach(var msg in msgs)
            {
                if (msg.Role == MessageRole.Tool)
                {
                    items.Add(new AssembledItem
                    {
                        Role = "assistant",
                        Type = "function_call",
                        ToolName = msg.ToolName,
                        Content = msg.Content,
                        ToolCallId = msg.ToolCallId,
                        Arguments = msg.ToolArgs
                    });
                    items.Add(new AssembledItem
                    {
                        Role = "tool",
                        Type = "function_call_output",
                        ToolCallId = msg.ToolCallId,
                        ToolName = msg.ToolName,
                        Content = msg.Content
                    });
                } else
                {
                    items.Add(new AssembledItem
                    {
                        Role = msg.Role.ToString().ToLower(),
                        Type = "message",
                        Content = msg.Content
                    });
                }
            }

            return items;
        }

        /// <summary>
        /// Builds the string that aggregates RAG and memory
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns> The complete memory and context prompt </returns>
        public string BuildContextBlock(LlmInvocation invocation)
        {

            var sb = new StringBuilder();

            // ############ Memory ############
            if (!string.IsNullOrWhiteSpace(invocation.Memory.Content))
            {
                sb.AppendLine("### LONG-TERM MEMORY ###");
                sb.AppendLine(invocation.Memory.Content);
                sb.AppendLine();
            }

            // ############ RAG ############
            if (invocation.Rag.RetrievedChunks.Any())
            {
                sb.AppendLine("### CONTEXT ###");

                foreach (var chunk in invocation.Rag.RetrievedChunks)
                {
                    sb.AppendLine($"[Source: {chunk.SourceId}] - {chunk.Text}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string AddContent(string original, string context)
        {
            if (string.IsNullOrEmpty(context)) return original;

            return $"{original}{Environment.NewLine}{context}";
        }
    }
}
