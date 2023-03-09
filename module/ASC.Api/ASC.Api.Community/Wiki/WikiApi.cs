/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Api.Attributes;
using ASC.Api.Wiki.Wrappers;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Community.Product;
using ASC.Web.Community.Wiki.Common;
using ASC.Web.Core.Users;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;

using Microsoft.Security.Application;

using File = ASC.Web.UserControls.Wiki.Data.File;

namespace ASC.Api.Community
{
    public partial class CommunityApi
    {
        private readonly WikiEngine _engine = new WikiEngine();

        /// <summary>
        /// Creates a new wiki page with the page name and content specified in the request.
        /// </summary>
        /// <short>Create a page</short>
        /// <param name="name">Page name</param>
        /// <param name="body">Page content</param>
        /// <returns>Page information</returns>
        /// <category>Wiki</category>
        [Create("wiki")]
        public PageWrapper CreatePage(string name, string body)
        {
            return new PageWrapper(_engine.CreatePage(new Page { PageName = name, Body = Sanitizer.GetSafeHtmlFragment(body) }));
        }

        /// <summary>
        /// Returns a list of all the pages from wiki or wiki category specified in the request.
        /// </summary>
        /// <short>Get pages</short>
        /// <section>Pages</section>
        /// <param name="category">Category name</param>
        /// <returns>Pages</returns>
        /// <category>Wiki</category>
        [Read("wiki")]
        public IEnumerable<PageWrapper> GetPages(string category)
        {
            return category == null
                ? _engine.GetPages().ConvertAll(x => new PageWrapper(x))
                : _engine.GetPages(category).ConvertAll(x => new PageWrapper(x));
        }

        /// <summary>
        /// Returns the detailed information about a wiki page with the name and version specified in the request.
        /// </summary>
        /// <short>Get a page</short>
        /// <section>Pages</section>
        /// <param name="name">Page name</param>
        /// <param name="version">Page version</param>
        /// <returns>Page information</returns>
        /// <category>Wiki</category>
        [Read("wiki/{name}")]
        public PageWrapper GetPage(string name, int? version)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException();

            var page = version != null ? _engine.GetPage(name, (int)version) : _engine.GetPage(name);

            if (page == null) throw new Exception("wiki page not found");

            return new PageWrapper(page);
        }

        /// <summary>
        /// Returns a list of history changes for a wiki page with the name specified in the request.
        /// </summary>
        /// <short>Get the page history</short>
        /// <section>Pages</section>
        /// <param name="page">Page name</param>
        /// <returns>List of history changes</returns>
        /// <category>Wiki</category>
        [Read("wiki/{page}/story")]
        public IEnumerable<PageWrapper> GetHistory(string page)
        {
            return _engine.GetPageHistory(page).ConvertAll(x => new PageWrapper(x));
        }

        /// <summary>
        /// Returns a list of wiki pages with the name matching the search query specified in the request.
        /// </summary>
        /// <short>Search pages by name</short>
        /// <section>Pages</section>
        /// <param name="name">Search query</param>
        /// <returns>List of pages</returns>
        /// <category>Wiki</category>
        [Read("wiki/search/byname/{name}")]
        public IEnumerable<PageWrapper> SearchPages(string name)
        {
            return _engine.SearchPagesByName(name).ConvertAll(x => new PageWrapper(x));
        }

        /// <summary>
        /// Returns a list of wiki pages with the content matching the search query specified in the request.
        /// </summary>
        /// <short>Search pages by content</short>
        /// <section>Pages</section>
        /// <param name="content">Search query</param>
        /// <returns>List of pages</returns>
        /// <category>Wiki</category>
        [Read("wiki/search/bycontent/{content}")]
        public IEnumerable<PageWrapper> SearchPagesByContent(string content)
        {
            return _engine.SearchPagesByContent(content).ConvertAll(x => new PageWrapper(x));
        }

        /// <summary>
        /// Updates a wiki page with the name and content specified in the request.
        /// </summary>
        /// <short>Update a page</short>
        /// <section>Pages</section>
        /// <param name="name">New page name</param>
        /// <param name="body">New page content</param>
        /// <returns>Page information</returns>
        /// <category>Wiki</category>
        [Update("wiki/{name}")]
        public PageWrapper UpdatePage(string name, string body)
        {
            return new PageWrapper(_engine.UpdatePage(new Page { PageName = name, Body = Sanitizer.GetSafeHtmlFragment(body) }));
        }

        /// <summary>
        /// Deletes a wiki page with the name specified in the request.
        /// </summary>
        /// <short>Delete a page</short>
        /// <section>Pages</section>
        /// <param name="name">Page name</param>
        /// <category>Wiki</category>
        [Delete("wiki/{name}")]
        public void DeletePage(string name)
        {
            _engine.RemovePage(name);
        }

        /// <summary>
        /// Creates a comment on the selected wiki page with the content specified in the request.
        /// </summary>
        /// <short>Create a wiki comment</short>
        /// <section>Comments</section>
        /// <param name="page">Page name</param>
        /// <param name="content">Comment text</param>
        /// <param name="parentId">Comment parent ID</param>
        /// <returns>Comment information</returns>
        /// <category>Wiki</category>
        [Create("wiki/{page}/comment")]
        public CommentWrapper CreateComment(string page, string content, string parentId)
        {
            var parentIdGuid = String.IsNullOrEmpty(parentId) ? Guid.Empty : new Guid(parentId);
            return new CommentWrapper(_engine.CreateComment(new Comment { PageName = page, Body = content, ParentId = parentIdGuid }));
        }

        /// <summary>
        /// Returns a list of all the comments to the wiki page with the name specified in the request.
        /// </summary>
        /// <short>Get the page comments</short>
        /// <section>Comments</section>
        /// <param name="page">Page name</param>
        /// <returns>List of comments</returns>
        /// <category>Wiki</category>
        [Read("wiki/{page}/comment")]
        public List<CommentWrapper> GetComments(string page)
        {
            return _engine.GetComments(page).ConvertAll(x => new CommentWrapper(x));
        }

        /// <summary>
        /// Uploads the selected files to the wiki 'Files' section.
        /// </summary>
        /// <short>Upload files</short>
        /// <param name="files">List of files</param>
        /// <returns>List of files</returns>
        /// <category>Wiki</category>
        [Create("wiki/file")]
        public IEnumerable<FileWrapper> UploadFiles(IEnumerable<System.Web.HttpPostedFileBase> files)
        {
            if (files == null || !files.Any()) throw new ArgumentNullException("files");

            return files.Select(file => new FileWrapper(_engine.CreateOrUpdateFile(new File { FileName = file.FileName, FileSize = file.ContentLength, UploadFileName = file.FileName }, file.InputStream)));
        }

        /// <summary>
        /// Returns the detailed information about a file with the name specified in the request from the wiki 'Files' section.
        /// </summary>
        /// <short>Get a file</short>
        /// <section>Files</section>
        /// <param name="name">File name</param>
        /// <returns>File information</returns>
        /// <category>Wiki</category>
        [Read("wiki/file/{name}")]
        public FileWrapper GetFile(string name)
        {
            return new FileWrapper(_engine.GetFile(name));
        }

        /// <summary>
        /// Deletes a file with the name specified in the request from the wiki 'Files' section.
        /// </summary>
        /// <short>Delete a file</short>
        /// <param name="name">File name</param>
        /// <category>Wiki</category>
        [Delete("wiki/file/{name}")]
        public void DeleteFile(string name)
        {
            _engine.RemoveFile(name);
        }

        /// <summary>
        /// Returns a comment preview with the content specified in the request.
        /// </summary>
        /// <short>Get a comment preview</short>
        /// <section>Comments</section>
        /// <param name="commentid">Comment ID</param>
        /// <param name="htmltext">Comment text in the HTML format</param>
        /// <returns>Comment information</returns>
        /// <category>Wiki</category>
        [Create("wiki/comment/preview")]
        public CommentInfo GetWikiCommentPreview(string commentid, string htmltext)
        {
            var comment = !string.IsNullOrEmpty(commentid) ? _engine.GetComment(new Guid(commentid)) : new Comment();
            comment.Date = TenantUtil.DateTimeNow();
            comment.UserId = SecurityContext.CurrentAccount.ID;
            comment.Body = htmltext;

            var info = GetCommentInfo(comment);
            info.IsEditPermissions = false;
            info.IsResponsePermissions = false;
            return info;
        }

        /// <summary>
        ///Removes a comment with the ID specified in the request.
        /// </summary>
        /// <short>Remove a comment</short>
        /// <section>Comments</section>
        /// <param name="commentid">Comment ID</param>
        /// <returns>Comment ID</returns>
        /// <category>Wiki</category>
        [Delete("wiki/comment/{commentid}")]
        public string RemoveWikiComment(string commentid)
        {
            _engine.DeleteComment(new Guid(commentid));
            return commentid;
        }



        /// <summary>
        /// Adds a comment to the selected entity with the content specified in the request.
        /// </summary>
        /// <short>Add an entity comment</short>
        /// <section>Comments</section>
        /// <param name="parentcommentid">Comment parent ID</param>
        /// <param name="entityid">Entity ID</param>
        /// <param name="content">Comment text</param>
        /// <returns>Comment information</returns>
        /// <category>Wiki</category>
        [Create("wiki/comment")]
        public CommentInfo AddWikiComment(string parentcommentid, string entityid, string content)
        {
            CommunitySecurity.DemandPermissions(ASC.Web.Community.Wiki.Common.Constants.Action_AddComment);


            var parentIdGuid = String.IsNullOrEmpty(parentcommentid) ? Guid.Empty : new Guid(parentcommentid);
            var newComment = _engine.CreateComment(new Comment { Body = content, PageName = entityid, ParentId = parentIdGuid });

            return GetCommentInfo(newComment);
        }

        /// <summary>
        /// Updates a comment on the selected wiki page with the content specified in the request.
        /// </summary>
        /// <short>Update a comment</short>
        /// <section>Comments</section>
        /// <param name="commentid">Comment ID</param>
        /// <param name="content">New comment text</param>
        /// <returns>Updated comment</returns>
        /// <category>Wiki</category>
        [Update("wiki/comment/{commentid}")]
        public string UpdateWikiComment(string commentid, string content)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentException();

            _engine.UpdateComment(new Comment { Id = new Guid(commentid), Body = content });
            return HtmlUtility.GetFull(content);
        }

        private static CommentInfo GetCommentInfo(Comment comment)
        {
            var info = new CommentInfo
            {
                CommentID = comment.Id.ToString(),
                UserID = comment.UserId,
                TimeStamp = comment.Date,
                TimeStampStr = comment.Date.Ago(),
                IsRead = true,
                Inactive = comment.Inactive,
                CommentBody = HtmlUtility.GetFull(comment.Body),
                UserFullName = DisplayUserSettings.GetFullUserName(comment.UserId),
                UserProfileLink = CommonLinkUtility.GetUserProfile(comment.UserId),
                UserAvatarPath = UserPhotoManager.GetBigPhotoURL(comment.UserId),
                IsEditPermissions = CommunitySecurity.CheckPermissions(new WikiObjectsSecurityObject(comment), ASC.Web.Community.Wiki.Common.Constants.Action_EditRemoveComment),
                IsResponsePermissions = CommunitySecurity.CheckPermissions(ASC.Web.Community.Wiki.Common.Constants.Action_AddComment),
                UserPost = CoreContext.UserManager.GetUsers(comment.UserId).Title
            };

            return info;
        }




    }
}
