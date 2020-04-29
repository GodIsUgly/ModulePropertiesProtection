using Confuser.Core;
using Confuser.Renamer;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Confuser.Protections
{
    public class ModulePropertiesProtection : Protection
    {
        public override ProtectionPreset Preset => ProtectionPreset.Maximum;

        public override string Name => "Module Properties";

        public override string Description => "Changes the properties of the module";

        public override string Id => "module properties";

        public override string FullId => "Ki.ModProperties";

        protected override void Initialize(ConfuserContext context) { }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.WriteModule, new ModulePropertiesPhase(this));
        }

        private class ModulePropertiesPhase : ProtectionPhase
        {
            public ModulePropertiesPhase(ModulePropertiesProtection parent) : base(parent) { }

            public override ProtectionTargets Targets => ProtectionTargets.Modules;

            public override string Name => "Module Properties";

            protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
            {
                var name = context.Registry.GetService<INameService>();

                foreach (ModuleDef module in parameters.Targets.OfType<ModuleDef>().WithProgress(context.Logger))
                {
                    bool isWPF = false;
                    foreach(AssemblyRef asmRef in module.GetAssemblyRefs())
                        if (asmRef.Name == "WindowsBase" || asmRef.Name == "PresentationCore" || asmRef.Name == "PresentationFramework" || asmRef.Name == "System.Xaml")
                            isWPF = true;
                    if (!isWPF)
                    {
                        module.Name = name.RandomName(RenameMode.ASCII);
                        module.Mvid = Guid.NewGuid();
                        module.Assembly.CustomAttributes.Clear();
                        module.Assembly.Name = name.RandomName();
                        module.Assembly.Version = new Version(r.Next(1, 9), r.Next(1, 9), r.Next(1, 9), r.Next(1, 9));
                    }
                    else
                        context.Logger.WarnFormat("Module Properties is not compatible with {0}.", module.Name);
                }
            }

            Random r = new Random(Guid.NewGuid().GetHashCode());
        }
    }
}
