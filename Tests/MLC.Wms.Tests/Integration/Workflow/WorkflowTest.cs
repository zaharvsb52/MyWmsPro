using System;
using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.General;

namespace MLC.Wms.Tests.Integration.Workflow
{
    [TestFixture]
    public class WorkflowTest
    {
        private const string TestProcessHostActivity = @"<Activity mc:Ignorable='sads sap' x:Class='wmsMLC.Business.Tests.TestProcessHostActivity'
 xmlns='http://schemas.microsoft.com/netfx/2009/xaml/activities'
 xmlns:av='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
 xmlns:mc='http://schemas.openxmlformats.org/markup-compatibility/2006'
 xmlns:mv='clr-namespace:Microsoft.VisualBasic;assembly=System'
 xmlns:mva='clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities'
 xmlns:s='clr-namespace:System;assembly=mscorlib'
 xmlns:s1='clr-namespace:System;assembly=System'
 xmlns:s2='clr-namespace:System;assembly=System.Xml'
 xmlns:s3='clr-namespace:System;assembly=System.Core'
 xmlns:s4='clr-namespace:System;assembly=System.ServiceModel'
 xmlns:sa='clr-namespace:System.Activities;assembly=System.Activities'
 xmlns:sad='clr-namespace:System.Activities.Debugger;assembly=System.Activities'
 xmlns:sads='http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger'
 xmlns:sap='http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation'
 xmlns:scg='clr-namespace:System.Collections.Generic;assembly=System'
 xmlns:scg1='clr-namespace:System.Collections.Generic;assembly=System.ServiceModel'
 xmlns:scg2='clr-namespace:System.Collections.Generic;assembly=System.Core'
 xmlns:scg3='clr-namespace:System.Collections.Generic;assembly=mscorlib'
 xmlns:sd='clr-namespace:System.Data;assembly=System.Data'
 xmlns:sl='clr-namespace:System.Linq;assembly=System.Core'
 xmlns:st='clr-namespace:System.Text;assembly=mscorlib'
 xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <x:Members>
    <x:Property Name='inputString' Type='InArgument(x:String)' />
    <x:Property Name='outputObject' Type='OutArgument(x:Object)' />
    <x:Property Name='inputObject' Type='InArgument(x:Object)' />
  </x:Members>
  <sap:VirtualizedContainerService.HintSize>654,716</sap:VirtualizedContainerService.HintSize>
  <mva:VisualBasic.Settings>Assembly references and imported namespaces for internal implementation</mva:VisualBasic.Settings>
  <Flowchart sad:XamlDebuggerXmlReader.FileName='d:\nosov_vg\src\wmsMLC\Sources\Tests\wmsMLC.Business.Tests\TestProcessHostActivity.xaml' sap:VirtualizedContainerService.HintSize='614,636'>
    <sap:WorkflowViewStateService.ViewState>
      <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
        <x:Boolean x:Key='IsExpanded'>False</x:Boolean>
        <av:Point x:Key='ShapeLocation'>270,2.5</av:Point>
        <av:Size x:Key='ShapeSize'>60,75</av:Size>
        <av:PointCollection x:Key='ConnectorLocation'>300,77.5 300,104.5</av:PointCollection>
      </scg3:Dictionary>
    </sap:WorkflowViewStateService.ViewState>
    <Flowchart.StartNode>
      <x:Reference>__ReferenceID1</x:Reference>
    </Flowchart.StartNode>
    <FlowStep x:Name='__ReferenceID1'>
      <sap:WorkflowViewStateService.ViewState>
        <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
          <av:Point x:Key='ShapeLocation'>200,104.5</av:Point>
          <av:Size x:Key='ShapeSize'>200,51</av:Size>
          <av:PointCollection x:Key='ConnectorLocation'>300,155.5 300,180</av:PointCollection>
        </scg3:Dictionary>
      </sap:WorkflowViewStateService.ViewState>
      <If Condition='[inputString = inputObject.ToString]' DisplayName='Сравнение входных аргументов' sap:VirtualizedContainerService.HintSize='464,209'>
        <sap:WorkflowViewStateService.ViewState>
          <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
            <x:Boolean x:Key='IsExpanded'>True</x:Boolean>
          </scg3:Dictionary>
        </sap:WorkflowViewStateService.ViewState>
        <If.Then>
          <WriteLine DisplayName='Аргументы равны' sap:VirtualizedContainerService.HintSize='211,61' Text='Равно' />
        </If.Then>
        <If.Else>
          <WriteLine DisplayName='Аргументы не равны' sap:VirtualizedContainerService.HintSize='211,61' Text='Не равно' />
        </If.Else>
      </If>
      <FlowStep.Next>
        <FlowStep x:Name='__ReferenceID0'>
          <sap:WorkflowViewStateService.ViewState>
            <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
              <av:Point x:Key='ShapeLocation'>179,180</av:Point>
              <av:Size x:Key='ShapeSize'>242,60</av:Size>
            </scg3:Dictionary>
          </sap:WorkflowViewStateService.ViewState>
          <Assign DisplayName='Выставление значения выходному аргументу' sap:VirtualizedContainerService.HintSize='242,60'>
            <Assign.To>
              <OutArgument x:TypeArguments='x:Object'>[outputObject]</OutArgument>
            </Assign.To>
            <Assign.Value>
              <InArgument x:TypeArguments='x:Object'>[1]</InArgument>
            </Assign.Value>
            <sap:WorkflowViewStateService.ViewState>
              <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
                <x:Boolean x:Key='IsExpanded'>True</x:Boolean>
              </scg3:Dictionary>
            </sap:WorkflowViewStateService.ViewState>
          </Assign>
        </FlowStep>
      </FlowStep.Next>
    </FlowStep>
    <x:Reference>__ReferenceID0</x:Reference>
  </Flowchart>
</Activity>";

        private const string TestProcessHostActivityWithIdle = @"<Activity mc:Ignorable='sads sap' x:Class='wmsMLC.Business.Tests.TestProcessHostActivityWithIdle'
 xmlns='http://schemas.microsoft.com/netfx/2009/xaml/activities'
 xmlns:av='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
 xmlns:mc='http://schemas.openxmlformats.org/markup-compatibility/2006'
 xmlns:mv='clr-namespace:Microsoft.VisualBasic;assembly=System'
 xmlns:mva='clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities'
 xmlns:s='clr-namespace:System;assembly=mscorlib'
 xmlns:s1='clr-namespace:System;assembly=System'
 xmlns:s2='clr-namespace:System;assembly=System.Xml'
 xmlns:s3='clr-namespace:System;assembly=System.Core'
 xmlns:s4='clr-namespace:System;assembly=System.ServiceModel'
 xmlns:sa='clr-namespace:System.Activities;assembly=System.Activities'
 xmlns:sad='clr-namespace:System.Activities.Debugger;assembly=System.Activities'
 xmlns:sads='http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger'
 xmlns:sap='http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation'
 xmlns:scg='clr-namespace:System.Collections.Generic;assembly=System'
 xmlns:scg1='clr-namespace:System.Collections.Generic;assembly=System.ServiceModel'
 xmlns:scg2='clr-namespace:System.Collections.Generic;assembly=System.Core'
 xmlns:scg3='clr-namespace:System.Collections.Generic;assembly=mscorlib'
 xmlns:sd='clr-namespace:System.Data;assembly=System.Data'
 xmlns:sl='clr-namespace:System.Linq;assembly=System.Core'
 xmlns:st='clr-namespace:System.Text;assembly=mscorlib'
 xmlns:wab='clr-namespace:wmsMLC.Activities.Bookmarks;assembly=wmsMLC.Activities.Bookmarks'
 xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
  <x:Members>
    <x:Property Name='inputString' Type='InArgument(x:String)' />
    <x:Property Name='inputObject' Type='InArgument(x:Object)' />
    <x:Property Name='outputObject' Type='OutArgument(x:Object)' />
  </x:Members>
  <sap:VirtualizedContainerService.HintSize>654,716</sap:VirtualizedContainerService.HintSize>
  <mva:VisualBasic.Settings>Assembly references and imported namespaces for internal implementation</mva:VisualBasic.Settings>
  <Flowchart sad:XamlDebuggerXmlReader.FileName='d:\nosov_vg\src\wmsMLC\Sources\Tests\wmsMLC.Business.Tests\TestProcessHostActivityWithIdle.xaml' sap:VirtualizedContainerService.HintSize='614,636'>
    <Flowchart.Variables>
      <Variable x:TypeArguments='x:Object' Name='internalObject' />
    </Flowchart.Variables>
    <sap:WorkflowViewStateService.ViewState>
      <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
        <x:Boolean x:Key='IsExpanded'>False</x:Boolean>
        <av:Point x:Key='ShapeLocation'>270,2.5</av:Point>
        <av:Size x:Key='ShapeSize'>60,75</av:Size>
        <av:PointCollection x:Key='ConnectorLocation'>300,77.5 300,104.5</av:PointCollection>
      </scg3:Dictionary>
    </sap:WorkflowViewStateService.ViewState>
    <Flowchart.StartNode>
      <x:Reference>__ReferenceID3</x:Reference>
    </Flowchart.StartNode>
    <FlowStep x:Name='__ReferenceID3'>
      <sap:WorkflowViewStateService.ViewState>
        <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
          <av:Point x:Key='ShapeLocation'>200,104.5</av:Point>
          <av:Size x:Key='ShapeSize'>200,51</av:Size>
          <av:PointCollection x:Key='ConnectorLocation'>300,155.5 300,189</av:PointCollection>
        </scg3:Dictionary>
      </sap:WorkflowViewStateService.ViewState>
      <If Condition='[inputString = inputObject.ToString]' DisplayName='Сравнение входных аргументов' sap:VirtualizedContainerService.HintSize='200,51'>
        <sap:WorkflowViewStateService.ViewState>
          <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
            <x:Boolean x:Key='IsExpanded'>True</x:Boolean>
          </scg3:Dictionary>
        </sap:WorkflowViewStateService.ViewState>
        <If.Then>
          <WriteLine DisplayName='Аргументы равны' sap:VirtualizedContainerService.HintSize='211,61' Text='Равно' />
        </If.Then>
        <If.Else>
          <WriteLine DisplayName='Аргументы не равны' sap:VirtualizedContainerService.HintSize='211,61' Text='Не равно' />
        </If.Else>
      </If>
      <FlowStep.Next>
        <FlowStep x:Name='__ReferenceID2'>
          <sap:WorkflowViewStateService.ViewState>
            <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
              <av:Point x:Key='ShapeLocation'>200,189</av:Point>
              <av:Size x:Key='ShapeSize'>200,22</av:Size>
              <av:PointCollection x:Key='ConnectorLocation'>300,211 300,260</av:PointCollection>
            </scg3:Dictionary>
          </sap:WorkflowViewStateService.ViewState>
          <wab:WaitForNextStepBookmark x:TypeArguments='x:Object' DisplayName='Ожидание следующего шага' sap:VirtualizedContainerService.HintSize='200,22' MyTestObject='[inputObject]' Result='[internalObject]'>
            <sap:WorkflowViewStateService.ViewState>
              <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
                <x:Boolean x:Key='IsExpanded'>True</x:Boolean>
              </scg3:Dictionary>
            </sap:WorkflowViewStateService.ViewState>
          </wab:WaitForNextStepBookmark>
          <FlowStep.Next>
            <FlowStep x:Name='__ReferenceID0'>
              <sap:WorkflowViewStateService.ViewState>
                <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
                  <av:Point x:Key='ShapeLocation'>179,260</av:Point>
                  <av:Size x:Key='ShapeSize'>242,60</av:Size>
                  <av:PointCollection x:Key='ConnectorLocation'>300,320 300,349.5</av:PointCollection>
                </scg3:Dictionary>
              </sap:WorkflowViewStateService.ViewState>
              <Assign DisplayName='Выставление значения выходному аргументу' sap:VirtualizedContainerService.HintSize='242,60'>
                <Assign.To>
                  <OutArgument x:TypeArguments='x:Object'>[outputObject]</OutArgument>
                </Assign.To>
                <Assign.Value>
                  <InArgument x:TypeArguments='x:Object'>[internalObject]</InArgument>
                </Assign.Value>
                <sap:WorkflowViewStateService.ViewState>
                  <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
                    <x:Boolean x:Key='IsExpanded'>True</x:Boolean>
                  </scg3:Dictionary>
                </sap:WorkflowViewStateService.ViewState>
              </Assign>
              <FlowStep.Next>
                <FlowStep x:Name='__ReferenceID1'>
                  <sap:WorkflowViewStateService.ViewState>
                    <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
                      <av:Point x:Key='ShapeLocation'>194.5,349.5</av:Point>
                      <av:Size x:Key='ShapeSize'>211,61</av:Size>
                    </scg3:Dictionary>
                  </sap:WorkflowViewStateService.ViewState>
                  <WriteLine DisplayName='Выводим значение выходного объекта' sap:VirtualizedContainerService.HintSize='211,61' Text='[outputObject.ToString]'>
                    <sap:WorkflowViewStateService.ViewState>
                      <scg3:Dictionary x:TypeArguments='x:String, x:Object'>
                        <x:Boolean x:Key='IsExpanded'>True</x:Boolean>
                      </scg3:Dictionary>
                    </sap:WorkflowViewStateService.ViewState>
                  </WriteLine>
                </FlowStep>
              </FlowStep.Next>
            </FlowStep>
          </FlowStep.Next>
        </FlowStep>
      </FlowStep.Next>
    </FlowStep>
    <x:Reference>__ReferenceID0</x:Reference>
    <x:Reference>__ReferenceID1</x:Reference>
    <x:Reference>__ReferenceID2</x:Reference>
  </Flowchart>
</Activity>";

        private const bool NotUseActivityStackTrace = false;
        private const bool NotUsePersistAndLog = false;

        Guid _id;
        IProcessHost _processHost;

        [SetUp]
        public void Setup()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);
            _id = Guid.NewGuid();
            _processHost = IoC.Instance.Resolve<IProcessHost>();
        }

        [Test]
        public void CreateAndRun()
        {
            var inputs = new Dictionary<string, object>();
            inputs.Add("inputString", "123");
            inputs.Add("inputObject", "123");
            _processHost.CreateAndRun(Guid.NewGuid(), TestProcessHostActivity, inputs);
        }

        [Test]
        public void CreateAndRunWithIdle()
        {
            var inputs = new Dictionary<string, object>();
            inputs.Add("inputString", "123");
            inputs.Add("inputObject", "123");

            _processHost.CreateAndRun(_id, TestProcessHostActivityWithIdle, inputs);
        }

        [Test]
        public void ContinueFromIdle()
        {
            var inputs = new Dictionary<string, object>();
            inputs.Add("inputObject", "123456");

            _processHost.CreateAndRun(_id, TestProcessHostActivityWithIdle, inputs);
        }
    }
}
