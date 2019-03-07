namespace DAQ.Service
{
    public class MsgDBSaver : IQueueProcesser<TestSpecViewModel>
    {
        QueueProcesser<TestSpecViewModel> processer;
        public string FolderName { get; set; } = "../DAQData/";
        public MsgDBSaver()
        {
            processer = new QueueProcesser<TestSpecViewModel>((s) =>
            {
                using (DataAccess db = new DataAccess())
                {
                    db.SaveTestSpecs(s);
                }
            });
        }

        public void Process( TestSpecViewModel msg)
        {
            processer.Process(msg);
        }
    }
}