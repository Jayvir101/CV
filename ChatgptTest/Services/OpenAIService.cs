using Azure;
using Azure.AI.OpenAI;
using ChatgptTest.Configurations;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ChatgptTest.Services
{
    public class OpenAIService
    {
        private readonly OpenAIClient _client;
        private readonly AzureOpenAIServiceSettings _settings;

        public OpenAIService(OpenAIClient client, IOptions<AzureOpenAIServiceSettings> settings)
        {
            _client = client;
            _settings = settings.Value;
        }

        public string GetOpenAIResponse(string name, string cvText)
        {
            string modelToUse = "gpt4-32k";
            string question = $"does this text \"{cvText}\" look like a resume of {name}? He has applied for a job at our firm. if it seems like a resume/cv reply 'YES' else give reason ";

            string reply = "AI Could not generate reply for your question!";

            try
            {
                var options = new ChatCompletionsOptions
                {
                    Messages = {
                        new ChatMessage(ChatRole.System, "You are an AI assistant that helps people find information."),
                        new ChatMessage(ChatRole.User, question)
                    },
                    Temperature = (float)_settings.TEMPERATURE,
                    MaxTokens = _settings.MAX_TOKENS,
                    NucleusSamplingFactor = (float)_settings.SAMPLING_FACTOR,
                    FrequencyPenalty = _settings.FREQUENCY_PENALTY,
                    PresencePenalty = _settings.PRESENCE_PENALTY,
                    ChoiceCount = _settings.SAMPLE_COUNT,
                };

                var completionsResponse = _client.GetChatCompletions(modelToUse, options);

                if (completionsResponse.Value.Choices.Count > 0)
                {
                    reply = completionsResponse.Value.Choices[0].Message.Content;
                }
            }
            catch (Exception e)
            {
                reply = e.Message;
            }

            return reply;
        }


        public string GetInfo(string cvText)
        {
            string modelToUse = "gpt4-32k";
            string question = $"This is a cv file of a person,{cvText},Generate a very brief summary for 1. Person, 2 His or her Qualification and 3. Professional Experience Output format:Person Details: Provide basic details Qualifications: Provide important once using Bullet Points Professional Experience: Provide important experiences using Bullet Points | Years | Role(do not include details)  ";

            string reply = "AI Could not generate reply for your question!";

            try
            {
                var options = new ChatCompletionsOptions
                {
                    Messages = {
                        new ChatMessage(ChatRole.System, "You are an AI assistant that helps people find information."),
                        new ChatMessage(ChatRole.User, question)
                    },
                    Temperature = (float)_settings.TEMPERATURE,
                    MaxTokens = _settings.MAX_TOKENS,
                    NucleusSamplingFactor = (float)_settings.SAMPLING_FACTOR,
                    FrequencyPenalty = _settings.FREQUENCY_PENALTY,
                    PresencePenalty = _settings.PRESENCE_PENALTY,
                    ChoiceCount = _settings.SAMPLE_COUNT,
                };

                var completionsResponse = _client.GetChatCompletions(modelToUse, options);

                if (completionsResponse.Value.Choices.Count > 0)
                {
                    reply = completionsResponse.Value.Choices[0].Message.Content+" Please try uploading another CV";
                }
            }
            catch (Exception e)
            {
                reply = e.Message;
            }

            return reply;
        }
    }
}

