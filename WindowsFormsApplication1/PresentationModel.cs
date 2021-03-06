﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanApp
{
    public class PresentationModel
    {

        private Model _model;
        private bool _isRegisterButtonEnable = true;

        //sticky用到的
        //private Task _targetTask;
        private bool _isEdit;

        private string _idText = "";
        private string _nickNameText = "";
        private string _memberIdText = "";
        private bool _addButtonEnable = false;
        private string _projectName = "None";

        private List<string> _membersName = new List<string>();
        private List<int> _membersState = new List<int>();
        //private 
        private string _errorString;

        // event
        public delegate void RefreshSticky(List<Task> todo, List<Task> doing, List<Task> done);
        public event RefreshSticky RefreshStickies;

        #region Properties
        public List<int> MembersState
        {
            get { return _membersState; }
            set { _membersState = value; }
        }

        public string ProjectName
        {
            get { return _projectName; }
            set { _projectName = value; }
        }
        public List<string> MembersName
        {
            get { return _membersName; }
            set { _membersName = value; }
        }
        public string ErrorString
        {
            get { return _errorString; }
        }
        public string NickNameText
        {
            get { return _nickNameText; }
            set { _nickNameText = value; }
        }

        public string IdText
        {
            get { return _idText; }
            set { _idText = value; }
        }
        public bool IsRegisterButtonEnable
        {
            get { return _isRegisterButtonEnable; }
            set { _isRegisterButtonEnable = value; }
        }

        #endregion

        public PresentationModel(Model model)
        {
            _model = model;
        }

        public void Init()
        {
            _model.Init();
            IsRegister();
        
        }

        public void Register()
        {
            int id = 0;
            _model.Register(_nickNameText, out id);
            _idText = id.ToString();
            //TODO(gca):model register here
        }

        public void CreateProject(string name)
        {
            _model.CreateProject(Int32.Parse(_idText), name);
        }

        public void AddMember(int memberId)
        {
            _model.AddMember(memberId);
            //TODO(gca): Refresh Listview
            UpdateProjectMembersList();
            UpdateProjectMembersState();
        }
        public void UpdateProjectTask()
        {
            _model.RefreshTaskList();
            RefreshStickies(_model.TodoList, _model.DoingList, _model.DoneList);
        }

        public void UpdateProjectName()
        {
            _projectName = _model.GetProjectName();
        }

        public void UpdateProjectMembersList()
        {
            _membersName = _model.GetMembersName();
        }

        public void UpdateProjectMembersState()
        {
            _membersState = _model.GetMembersState();
        }

        public void UpdateUserState(bool state)
        {
            _model.ChangeUserState(state);
        }

        /// <summary>
        /// 判斷是否有註冊過，註冊過將會更新ID跟NickName
        /// </summary>
        private void IsRegister()
        {
            //update idtext
            if (_model.IsRegister(out _idText, out _nickNameText))
            {
                _isRegisterButtonEnable = false;
                _nickNameText = _model.Database.GetNickName(_idText);
                //TODO(gca):need to update id and nick name 
            }
            else _isRegisterButtonEnable = true;
        }

        //private void ShowProjectMember()
        //{
        //    _membersName = _model.GetMembersName();
        //}



        public bool AddorEditTask(string title, string assignee, int priority, string deadline, string description)
        {

            if (title == "" || assignee == "" || priority < 1 || priority > 5 || description == "")
            {
                _errorString = "尚有項目未填寫";
                return false;
            }
            if (!_isEdit)
            {
                if (!_model.AddTask(title, assignee, priority, deadline, description))
                {
                    _errorString = "目前資料庫有問題，請稍後再試一次";
                    return false;

                }
                //_targetTask = _model.TargetTask;
                RefreshStickies(_model.TodoList, _model.DoingList, _model.DoneList);
                return true;
            }
            else
            {
                if (!_model.EditTask(title, assignee, priority, deadline, description))
                {
                    _errorString = "目前資料庫有問題，請稍後再試一次";
                    return false;

                }
                //_targetTask = _model.TargetTask;
                RefreshStickies(_model.TodoList, _model.DoingList, _model.DoneList);
                return true;
            }

        }
        //欲修改前須要先設定目標工作
        public void SetEditState(Task task)
        {
            _isEdit = true;
            _model.TargetTask = task;
            RefreshStickies(_model.TodoList, _model.DoingList, _model.DoneList);
        }

        //取消修改狀態並且設定工作目標為null
        public void ConcelEditState()
        {
            _isEdit = false;
            _model.TargetTask = null;
            RefreshStickies(_model.TodoList, _model.DoingList, _model.DoneList);
        }

        //更改工作狀態(使用前請先設定目標工作)
        public bool ChangeTaskState(Task task, TaskState destinationState)
        {
            if (_isEdit)
            {
                bool isSuccess = _model.ChangeTaskState( destinationState);
                if (!isSuccess) _errorString = "目前資料庫有問題，請稍後再試一次";
                else
                {
                    RefreshStickies(_model.TodoList, _model.DoingList, _model.DoneList);
                }
                return isSuccess;
            }
            else
            {
                _errorString = "無指定項目";
                return false;
            }

        }

        //回傳目標任務
        public Task GetTargetTask()
        {
            return _model.TargetTask;
        }

        //確認現在是更改的狀態
        public bool IsEdit()
        {
            return _isEdit;
        }

        public List<String> GetProjectUsers()
        {
            return _model.GetMembersName();
        }
        
    }
}
