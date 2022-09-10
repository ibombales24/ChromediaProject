using Newtonsoft.Json;

namespace ChromediaProject
{
    public class JsonResult
    {
        public int page { get; set; }
        public int per_page { get; set; }
        public int total { get; set; }
        public int total_pages { get; set; }
        public IEnumerable<ArticleData> data { get; set; }
    }

    public class ArticleData
    {
        public string? title { get; set; }
        public string url { get; set; }
        public string author { get; set; }
        public int? num_comments { get; set; }
        public string? story_id { get; set; }
        public string? story_title { get; set; }
        public string? story_url { get; set; }
        public string? parent_id { get; set; }
        public string created_at { get; set; }
    }

    public class Result
    {
        public int numComments { get; set; }
        public string articleName { get; set; }
    }

    class Program
    {
        static async Task<string[]> topArticles(int limit)
        {
            List<Result> results = new List<Result>();
            string[] articleNames = new string[limit];
            int total_pages = 1;
            string apiUri = "https://jsonmock.hackerrank.com/api/articles?page=";
            HttpClient client = new HttpClient();

            try
            {
                //Get 1st page result and total_pages
                HttpResponseMessage response = await client.GetAsync($"{apiUri}1");

                string contentString = await response.Content.ReadAsStringAsync();
                var parsedJson = JsonConvert.DeserializeObject<JsonResult>(contentString);
                results.AddRange(parsedJson.data.Select(a => new Result
                {
                    articleName = a.title ?? a.story_title ?? "",
                    numComments = a.num_comments ?? 0
                }));
                total_pages = parsedJson.total_pages;

                //If more than one page, get other pages asynchronously
                if (total_pages > 1)
                {
                    List<Task<IEnumerable<Result>>> tasks = new List<Task<IEnumerable<Result>>>();
                    for (int i = 2; i <= total_pages; i++)
                    {
                        async Task<IEnumerable<Result>> func()
                        {
                            HttpResponseMessage response = await client.GetAsync($"{apiUri}{i}");

                            string contentString = await response.Content.ReadAsStringAsync();
                            var parsedJson = JsonConvert.DeserializeObject<JsonResult>(contentString);
                            return parsedJson.data.Select(a => new Result
                            {
                                articleName = a.title ?? a.story_title ?? "",
                                numComments = a.num_comments ?? 0
                            });
                        }
                        tasks.Add(func());
                    }

                    //wait for all tasks to finish
                    await Task.WhenAll(tasks);

                    //add each task result to results list
                    foreach (var task in tasks)
                    {
                        results.AddRange(task.Result);
                    }
                }

                //Sort and Take article names
                articleNames = results
                    .Where(a => a.articleName != "")
                    .OrderByDescending(keySelector: a => a.numComments)
                    .ThenByDescending(keySelector: a => a.articleName)
                    .Select(a => a.articleName)
                    .Take(limit)
                    .ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return articleNames;
        }

        static void Main(string[] args)
        {
            var articleNames = topArticles(10).Result;
            foreach(var articleName in articleNames)
            {
                Console.WriteLine(articleName);
            }
        }
    }
}