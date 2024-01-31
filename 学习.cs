using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.GraphicsSystem;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 学习
{
    internal class 定义CAD命令
    {
        [CommandMethod("定义CAD命令的标志和选项", CommandFlags.UsePickSet)]
        public void 定义CAD命令的标志和选项()
        {

            // 枚举类型CommandFlags
            //Modal = 0x0, 指示命令以模态方式执行。模态命令会阻止用户在执行期间进行其他操作。
            //Transparent = 0x1, 指示命令在透明命令模式下执行。透明命令模式允许命令在不中断当前命令的情况下执行。
            //UsePickSet = 0x2, 指示命令应使用当前选择集作为其操作的对象。选择集是用户在CAD界面中选择的对象集合。
            //Redraw = 0x4, 指示命令执行后应重新绘制屏幕。这对于需要更新CAD界面以显示结果的命令很有用。
            //NoPerspective = 0x8, 指示命令在执行期间不应用透视效果。透视效果是一种用于展示三维场景的技术。
            //NoMultiple = 0x10, 指示命令不应允许多个实例同时存在。这可以防止用户在同一时间启动多个相同的命令。
            //NoTileMode = 0x20, 指示命令在瓦片模式下不可用。瓦片模式是CAD中用于分割视口的一种显示方式。
            //NoPaperSpace = 0x40, 指示命令在纸空间中不可用。纸空间是CAD中用于布局和打印的一种工作区域。
            //NoOem = 0x100, 指示命令不应使用OEM（Original Equipment Manufacturer）扩展功能。
            //Undefined = 0x200, 保留标志，未定义具体含义。
            //InProgress = 0x400, 指示命令正在进行中。
            //Defun = 0x800, 指示命令是一个DEFUN命令。DEFUN命令是AutoLISP编程语言中的用户自定义命令。
            //NoNewStack = 0x10000, 指示命令在执行期间不会创建新的调用堆栈。
            //NoInternalLock = 0x20000, 指示命令在执行期间不会进行内部锁定。
            //DocReadLock = 0x80000, 指示命令在执行期间会对文档进行读取锁定。
            //DocExclusiveLock = 0x100000, 指示命令在执行期间会对文档进行排他性锁定。
            //Session = 0x200000, 指示命令在会话级别执行，而不是在文档级别执行。
            //Interruptible = 0x400000, 指示命令可以被中断。这允许用户在命令执行期间进行其他操作。
            //NoHistory = 0x800000, 指示命令执行后不应将其添加到命令历史记录中。
            //NoUndoMarker = 0x1000000, 指示命令执行后不应在命令历史记录中创建撤消标记。
            //NoBlockEditor = 0x2000000, 指示命令在块编辑器中不可用。块编辑器是CAD中用于编辑块定义的工具。
            //NoActionRecording = 0x4000000, 指示命令在动作记录期间不应被记录。
            //ActionMacro = 0x8000000, 指示命令是一个动作宏，可以在动作宏中使用。
            //NoInferConstraint = 0x40000000, 指示命令在执行期间不应推断约束。
            //TempShowDynDimension = int.MinValue

        }


    }
}
