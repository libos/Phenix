using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;

using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Phenix.Core;
using System.Diagnostics;
namespace Phenix
{
    /// <summary>
    /// NewTask.xaml 的交互逻辑
    /// </summary>
    public partial class NewTask : Window
    {
        private bool isFiredNormal = true;
        public Task aTask;
        public NewTask()
        {
            InitializeComponent();
            this.aTask = new Task();
            aTask.List = new List<Step>();
            aTask.created_at = DateTime.UtcNow;
            aTask.task_unique_no = aTask.generateUniqueID(Utils.MacAddress);
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete,Delete,CanDelete));
            CommandBindings.Add(new CommandBinding(ComponentCommands.MoveDown, MoveDown, CanMoveDown));
            CommandBindings.Add(new CommandBinding(ComponentCommands.MoveUp,MoveUp,CanMoveUp));
            this.stepList.IsEnabled = false;
        }


        private void textBox2_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            
            int result;
            if(!(int.TryParse(e.Text, out result)))
            {
                e.Handled = true;
            }
            
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if(!this.stepList.IsEnabled)
            {
                this.notask_lb.Visibility = Visibility.Hidden;
                this.stepList.IsEnabled = true;
            }

            Step aStep = Step.CreateStep();
            aStep.inputParams = new List<Param>();
            aStep.libs = new List<string>();
            aStep.setStepNo = this.aTask.Count;
            this.stepList.Items.Add("Step No." + aStep.StepNo);           
            
            this.aTask.Add(aStep);
            this.propertyGrid1.SelectedObject = this.aTask.Last;
        }

        private void stepList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isFiredNormal)
                this.propertyGrid1.SelectedObject = this.aTask[stepList.SelectedIndex];
        }

        private void Delete(object sender, ExecutedRoutedEventArgs e)
        {
            isFiredNormal = false;
            int stepNO = stepList.SelectedIndex;
            aTask.Remove(stepNO);
            int  index  = 0; 
            foreach (Step step in aTask.List)
            {
                step.setStepNo = index++;
            }
            stepList.Items.Remove(e.Parameter);
            isFiredNormal = true;
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter != null;
        }

        private void MoveDown(object sender, ExecutedRoutedEventArgs e)
        {
            isFiredNormal = false;
            var selectedItem = e.Parameter;
            var index = this.stepList.Items.IndexOf(selectedItem);

            Step tmpStep = aTask[index];
            aTask[index].setStepNo = aTask[index].StepNo + 1;
            aTask[index + 1].setStepNo = aTask[index + 1].StepNo - 1;
            aTask[index] = aTask[index + 1];
            aTask[index + 1] = (Step)tmpStep;


            stepList.Items.RemoveAt(index);
            stepList.Items.Insert(++index, selectedItem);
            stepList.SelectedIndex = index;
            propertyGrid1.SelectedObject = tmpStep;
            isFiredNormal = true;
        }

        private void CanMoveDown(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null && stepList.Items.IndexOf(e.Parameter) < (stepList.Items.Count - 1))
                e.CanExecute = true;
        }

        private void MoveUp(object sender, ExecutedRoutedEventArgs e)
        {
            isFiredNormal = false;
            var selectedItem = e.Parameter;
            var index = stepList.Items.IndexOf(selectedItem);

            Step tmpStep = aTask[index];
            aTask[index].setStepNo = aTask[index].StepNo - 1;
            aTask[index - 1].setStepNo = aTask[index - 1].StepNo + 1;
            aTask[index] = aTask[index - 1];
            aTask[index - 1] = tmpStep;

            stepList.Items.RemoveAt(index);
            stepList.Items.Insert(--index, selectedItem);
            stepList.SelectedIndex = index;

            propertyGrid1.SelectedObject = tmpStep;
            isFiredNormal = true;
        }

        private void CanMoveUp(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter != null && stepList.Items.IndexOf(e.Parameter) > 0)
                e.CanExecute = true;
        }

        private void create_Click(object sender, RoutedEventArgs e)
        {
            if (this.stepList.Items.Count < 1)
            {
                MessageBox.Show("无步骤，无法创建任务","错误",MessageBoxButton.OK,MessageBoxImage.Error);
                cancel_Click(sender, e);
            }
            if (Convert.ToInt16(task_need.Text) < 1)
            {
                error2.Visibility = Visibility.Visible;
            }
            else if (task_name.Text.Length < 1)
            {
                error1.Visibility = Visibility.Visible;
            }
            else
            {
                aTask.Name = task_name.Text;
                aTask.NeedNum = Convert.ToInt16(task_need.Text);
                this.DialogResult = true;
            }
        }

        private void task_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (task_name.Text.Length > 1)
            {
                error1.Visibility = Visibility.Hidden;

            }
            else
            {
                error1.Visibility = Visibility.Visible;
            }
        }

        private void task_need_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (task_need.Text.Length < 1)
            {
                return;
            }
            int result;
            int.TryParse(task_need.Text, out result);
            if (result < 1)
            {
                this.error2.Visibility = Visibility.Visible;
            }
            else
            {
                if(error2 != null)
                    this.error2.Visibility = Visibility.Hidden;
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {

        }

/*//NB
        private object CreateNewItem(Type type)
        {
            return Activator.CreateInstance(type);
        }
        */
    }
}
