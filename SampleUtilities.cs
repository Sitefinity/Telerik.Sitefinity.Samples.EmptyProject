using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Web.Security;
using System.Web.UI;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Abstractions.VirtualPath.Configuration;
using Telerik.Sitefinity.Blogs.Model;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.ContentLinks;
using Telerik.Sitefinity.Data.Linq.Dynamic;
using Telerik.Sitefinity.Data.Metadata;
using Telerik.Sitefinity.Forms.Model;
using Telerik.Sitefinity.Forums;
using Telerik.Sitefinity.Forums.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Metadata.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Model.ContentLinks;
using Telerik.Sitefinity.Modules.Blogs;
using Telerik.Sitefinity.Modules.Blogs.Configuration;
using Telerik.Sitefinity.Modules.Events.Configuration;
using Telerik.Sitefinity.Modules.Forms;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Modules.Libraries.Configuration;
using Telerik.Sitefinity.Modules.News;
using Telerik.Sitefinity.Modules.News.Configuration;
using Telerik.Sitefinity.Modules.Newsletters;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.Modules.Pages.Configuration;
using Telerik.Sitefinity.News.Model;
using Telerik.Sitefinity.Newsletters.Model;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.Publishing;
using Telerik.Sitefinity.Publishing.Model;
using Telerik.Sitefinity.Publishing.Pipes;
using Telerik.Sitefinity.Publishing.PublishingPoints;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Claims;
using Telerik.Sitefinity.Security.Configuration;
using Telerik.Sitefinity.Security.Model;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Taxonomies;
using Telerik.Sitefinity.Taxonomies.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Web.Configuration;
using Telerik.Sitefinity.Web.Model;
using Telerik.Sitefinity.Web.UI;
using Telerik.Sitefinity.Web.UI.ContentUI.Config;
using Telerik.Sitefinity.Services.Comments;
using Telerik.Sitefinity.Services.Comments.Proxies;

namespace Telerik.Sitefinity.Samples.Common
{
    public class SampleUtilities
    {
        public const string UrlNameCharsToReplace = @"[^\w\-\!\$\'\(\)\=\@\d_]+";

        public const string UrlNameReplaceString = "-";

        private static string GenerateUniqueControlIdForPage(PageDraft pageNode, string culture)
        {
            int controlsCount = 0;

            if (pageNode != null)
            {
                controlsCount = pageNode.Controls.Count;
            }

            string cultureSufix = (string.IsNullOrEmpty(culture)) ? string.Empty : string.Format("_" + culture);

            return String.Format("C" + controlsCount.ToString().PadLeft(3, '0') + cultureSufix);
        }

        private static string GenerateUniqueControlIdForTemplate(TemplateDraft template)
        {
            int controlsCount = 0;

            if (template != null)
            {
                controlsCount = template.Controls.Count;
            }

            return String.Format("T" + controlsCount.ToString().PadLeft(3, '0'));
        }

        private static string GenerateUniqueControlIdForForm(FormDraft form)
        {
            int controlsCount = 0;

            if (form != null)
            {
                controlsCount = form.Controls.Count;
                return String.Format(form.Name + "_C" + controlsCount.ToString().PadLeft(3, '0'));
            }

            return String.Empty;
        }

        public static void AddControlToPage(Guid pageId, Control control, string placeHolder, string caption)
        {
            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var page = pageManager.GetPageNodes().Where(p => p.Id == pageId).SingleOrDefault();

                if (page != null)
                {
                    var master = pageManager.EditPage(page.Page.Id);

                    if (master != null)
                    {
                        if (string.IsNullOrEmpty(control.ID))
                        {
                            control.ID = GenerateUniqueControlIdForPage(master, null);
                        }

                        var pageControl = pageManager.CreateControl<PageDraftControl>(control, placeHolder);
                        pageControl.Caption = caption;
                        pageControl.SiblingId = GetLastControlInPlaceHolderInPageId(master, placeHolder);
                        pageManager.SetControlDefaultPermissions(pageControl);
                        master.Controls.Add(pageControl);
                        master = pageManager.PagesLifecycle.CheckIn(master);
                        master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        pageManager.PagesLifecycle.Publish(master);
                        pageManager.SaveChanges();
                    }
                }
            }
        }

        public static void AddControlToPage(Guid pageId, string controlPath, string placeHolder, string caption)
        {
            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var page = pageManager.GetPageNodes().Where(p => p.Id == pageId).SingleOrDefault();

                if (page != null)
                {
                    var master = pageManager.EditPage(page.Page.Id);

                    if (master != null)
                    {
                        var control = BuildManager.CreateInstanceFromVirtualPath(controlPath, typeof(UserControl)) as UserControl;

                        if (control != null)
                        {

                            if (string.IsNullOrEmpty(control.ID))
                            {
                                control.ID = GenerateUniqueControlIdForPage(master, null);
                            }

                            var pageControl = pageManager.CreateControl<PageDraftControl>();
                            pageControl.ObjectType = controlPath;
                            pageControl.PlaceHolder = placeHolder;
                            pageManager.ReadProperties(control, pageControl);
                            pageControl.Caption = caption;
                            pageControl.SiblingId = GetLastControlInPlaceHolderInPageId(master, placeHolder);
                            pageManager.SetControlDefaultPermissions(pageControl);
                            master.Controls.Add(pageControl);

                            master = pageManager.PagesLifecycle.CheckIn(master);
                            master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                            pageManager.PagesLifecycle.Publish(master);
                            pageManager.SaveChanges();
                        }
                    }
                }
            }
        }

        public static void AddControlToPage(Guid pageId, PageControl control, string placeHolder)
        {
            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var page = pageManager.GetPageNodes().Where(p => p.Id == pageId).SingleOrDefault();

                if (page != null)
                {
                    control.SiblingId = GetLastControlInPlaceHolderInPageId(page, placeHolder);
                    pageManager.SetControlDefaultPermissions(control);
                    page.Page.Controls.Add(control);

                    var master = pageManager.EditPage(page.Page.Id);
                    master = pageManager.PagesLifecycle.CheckIn(master);
                    master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                    pageManager.PagesLifecycle.Publish(master);

                    pageManager.SaveChanges();
                }
            }
        }

        public static void AddControlToLocalizedPage(Guid pageId, Control widget, string placeHolder, string caption, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            PageManager pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var initialPageNode = pageManager.GetPageNode(pageId);
                var pageNode = GetPageNodeForLanguage(initialPageNode.Page, cultureInfo);
                var pageData = pageNode.Page;
                var master = pageManager.EditPage(pageData.Id);

                if (master != null)
                {
                    if (string.IsNullOrEmpty(widget.ID))
                    {
                        widget.ID = GenerateUniqueControlIdForPage(master, culture);
                    }

                    var control = pageManager.CreateControl<PageDraftControl>(widget, placeHolder);
                    control.Caption = caption;
                    control.SiblingId = GetLastControlInPlaceHolderInPageId(master, placeHolder);
                    pageManager.SetControlDefaultPermissions(control);
                    master.Controls.Add(control);

                    master = pageManager.PagesLifecycle.CheckIn(master);
                    master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                    pageManager.PagesLifecycle.Publish(master);

                    pageManager.SaveChanges();
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;
        }

        public static void AddControlToLocalizedPage(Guid pageId, string controlPath, string placeHolder, string caption, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            PageManager pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var initialPageNode = pageManager.GetPageNode(pageId);
                var pageNode = GetPageNodeForLanguage(initialPageNode.Page, cultureInfo);

                var pageData = pageNode.Page;
                var master = pageManager.EditPage(pageData.Id);

                if (master != null)
                {
                    var control = BuildManager.CreateInstanceFromVirtualPath(controlPath, typeof(UserControl)) as UserControl;

                    if (control != null)
                    {

                        if (string.IsNullOrEmpty(control.ID))
                        {
                            control.ID = GenerateUniqueControlIdForPage(master, culture);
                        }

                        var pageControl = pageManager.CreateControl<PageDraftControl>(false);
                        pageControl.ObjectType = controlPath;
                        pageControl.PlaceHolder = placeHolder;
                        pageManager.ReadProperties(control, pageControl);
                        pageControl.Caption = caption;
                        pageControl.SiblingId = GetLastControlInPlaceHolderInPageId(master, placeHolder);
                        pageManager.SetControlDefaultPermissions(pageControl);

                        master.Controls.Add(pageControl);

                        master = pageManager.PagesLifecycle.CheckIn(master);
                        master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        pageManager.PagesLifecycle.Publish(master);

                        pageManager.SaveChanges();
                    }
                }
            }
            Thread.CurrentThread.CurrentUICulture = currentCulture;
        }

        public static void AddControlToTemplate(Guid templateId, Control control, string placeHolder, string caption)
        {
            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var template = pageManager.GetTemplates().Where(t => t.Id == templateId).SingleOrDefault();

                if (template != null)
                {
                    var master = pageManager.TemplatesLifecycle.Edit(template);
                    var temp = pageManager.TemplatesLifecycle.CheckOut(master);

                    if (temp != null)
                    {
                        if (string.IsNullOrEmpty(control.ID))
                        {
                            control.ID = GenerateUniqueControlIdForTemplate(temp);
                        }

                        var templateControl = pageManager.CreateControl<TemplateDraftControl>(control, placeHolder);
                        templateControl.Caption = caption;
                        templateControl.SiblingId = GetLastControlInPlaceHolderInTemplateId(temp, placeHolder);

                        pageManager.SetControlDefaultPermissions(templateControl);

                        temp.Controls.Add(templateControl);

                        master = pageManager.TemplatesLifecycle.CheckIn(temp);
                        master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        pageManager.TemplatesLifecycle.Publish(master);

                        pageManager.SaveChanges();
                    }
                }
            }
        }

        public static void AddControlToTemplate(Guid templateId, string controlPath, string placeHolder, string caption)
        {
            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var template = pageManager.GetTemplates().Where(t => t.Id == templateId).SingleOrDefault();

                if (template != null)
                {
                    var master = pageManager.EditTemplate(template.Id);
                    var temp = pageManager.TemplatesLifecycle.CheckOut(master);

                    if (temp != null)
                    {
                        var control = BuildManager.CreateInstanceFromVirtualPath(controlPath, typeof(UserControl)) as UserControl;

                        if (control != null)
                        {

                            if (string.IsNullOrEmpty(control.ID))
                            {
                                control.ID = GenerateUniqueControlIdForTemplate(temp);
                            }

                            var templateControl = pageManager.CreateControl<TemplateDraftControl>();
                            templateControl.ObjectType = controlPath;
                            templateControl.PlaceHolder = placeHolder;
                            pageManager.ReadProperties(control, templateControl);
                            templateControl.Caption = caption;
                            templateControl.SiblingId = GetLastControlInPlaceHolderInTemplateId(temp, placeHolder);
                            pageManager.SetControlDefaultPermissions(templateControl);
                            temp.Controls.Add(templateControl);

                            master = pageManager.TemplatesLifecycle.CheckIn(temp);
                            master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                            pageManager.TemplatesLifecycle.Publish(master);
                            pageManager.SaveChanges();
                        }
                    }
                }
            }
        }

        public static string AddLocalizedControlToTemplate(Guid templateId, Control control, string placeHolder, string caption, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            string controlId = string.Empty;

            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var template = pageManager.GetTemplates().Where(t => t.Id == templateId).SingleOrDefault();

                if (template != null)
                {
                    var master = pageManager.EditTemplate(template.Id, cultureInfo);
                    var temp = pageManager.TemplatesLifecycle.CheckOut(master, cultureInfo);

                    if (temp != null)
                    {
                        if (string.IsNullOrEmpty(control.ID))
                        {
                            control.ID = GenerateUniqueControlIdForTemplate(temp);
                            controlId = control.ID;
                        }

                        var templateControl = pageManager.CreateControl<TemplateDraftControl>(control, placeHolder);
                        templateControl.Caption = caption;
                        templateControl.SiblingId = GetLastControlInPlaceHolderInTemplateId(temp, placeHolder);
                        pageManager.SetControlDefaultPermissions(templateControl);

                        pageManager.CopyProperties(templateControl, CultureInfo.InvariantCulture, cultureInfo);

                        temp.Controls.Add(templateControl);
                        master = pageManager.TemplatesLifecycle.CheckIn(temp);
                        master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        pageManager.TemplatesLifecycle.Publish(master);
                        pageManager.SaveChanges();
                    }
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;

            return controlId;
        }

        public static void UpdateLocalizedControlInTemplate(string controlId, Guid templateId, Dictionary<string, object> properties, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var template = pageManager.GetTemplates().Where(t => t.Id == templateId).SingleOrDefault();

                if (template != null)
                {
                    var master = pageManager.EditTemplate(template.Id, cultureInfo);
                    var temp = pageManager.TemplatesLifecycle.CheckOut(master, cultureInfo);

                    if (master != null)
                    {
                        var controlData = temp.Controls.Where(c => c.Properties.Where(p => p.Name == "ID" && p.Value == controlId).Count() > 0).SingleOrDefault();

                        if (controlData != null)
                        {
                            var c1 = pageManager.LoadControl(controlData, cultureInfo);

                            SetProperties(c1, properties);

                            pageManager.ReadProperties(c1, controlData, cultureInfo);

                            master = pageManager.TemplatesLifecycle.CheckIn(temp);
                            master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                            pageManager.TemplatesLifecycle.Publish(master);
                            pageManager.SaveChanges();
                        }
                    }
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;
        }

        public static string AddControlToLocalizedForm(Guid formId, Control control, string placeHolder, string caption, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var formsManager = FormsManager.GetManager();
            using (new ElevatedModeRegion(formsManager))
            {
                var form = formsManager.GetForms().Where(f => f.Id == formId).SingleOrDefault();

                if (form != null)
                {
                    var master = formsManager.Lifecycle.Edit(form, cultureInfo);
                    var temp = formsManager.Lifecycle.CheckOut(master, cultureInfo);

                    if (String.IsNullOrEmpty(control.ID))
                    {
                        control.ID = GenerateUniqueControlIdForForm(temp);
                    }

                    var formControl = formsManager.CreateControl<FormDraftControl>(control, placeHolder);

                    formControl.SiblingId = GetLastControlInPlaceHolderInFormId(temp, placeHolder);
                    formControl.Caption = caption;
                    formControl.OriginalControlId = Guid.NewGuid();

                    formControl.SupportedPermissionSets = formControl.IsLayoutControl ? ControlData.LayoutPermissionSets : ControlData.ControlPermissionSets;
                    formControl.SetDefaultPermissions(formsManager);

                    formsManager.CopyProperties(formControl, CultureInfo.InvariantCulture, cultureInfo);

                    temp.Controls.Add(formControl);

                    master = formsManager.Lifecycle.CheckIn(temp);
                    master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                    formsManager.Lifecycle.Publish(master);

                    formsManager.SaveChanges(true);
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;

            return control.ID;
        }

        public static void UpdateControlInLocalizedForm(string controlId, Guid formId, Dictionary<string, object> properties, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var formsManager = FormsManager.GetManager();
            using (new ElevatedModeRegion(formsManager))
            {
                var form = formsManager.GetForms().Where(f => f.Id == formId).SingleOrDefault();

                if (form != null)
                {
                    var master = formsManager.Lifecycle.Edit(form, cultureInfo);
                    var temp = formsManager.Lifecycle.CheckOut(master, cultureInfo);

                    var controlData = temp.Controls.Where(c => c.Properties.Where(p => p.Name == "ID" && p.Value == controlId).Count() > 0).SingleOrDefault();

                    if (controlData != null)
                    {
                        var c1 = formsManager.LoadControl(controlData, cultureInfo);

                        SetProperties(c1, properties);

                        formsManager.ReadProperties(c1, controlData, cultureInfo);
                    }

                    master = formsManager.Lifecycle.CheckIn(temp);
                    master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                    formsManager.Lifecycle.Publish(master);

                    formsManager.SaveChanges(true);
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;
        }

        public static void CreateAlbum(Guid id, string albumName)
        {
            var librariesManager = LibrariesManager.GetManager();
            using (new ElevatedModeRegion(librariesManager))
            {
                var album = librariesManager.GetAlbums().Where(a => a.Title == albumName).SingleOrDefault();

                if (album == null)
                {
                    album = librariesManager.CreateAlbum(id);
                    album.Title = albumName;

                    librariesManager.SaveChanges();
                }
            }
        }

        public static bool CreateBasedOnTemplate(Guid baseTemplateId, Guid newTemplateId, string name, string title, string theme)
        {
            bool result = false;

            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var template = pageManager.GetTemplates().Where(t => t.Id == baseTemplateId).SingleOrDefault();

                var newTemplate = pageManager.GetTemplates().Where(t => t.Id == newTemplateId).SingleOrDefault();

                if (newTemplate == null)
                {
                    var pageTemplate = pageManager.CreateTemplate(newTemplateId);

                    pageTemplate.Name = name;
                    pageTemplate.Title = title;
                    pageTemplate.ParentTemplate = template;
                    pageTemplate.Theme = theme;

                    var master = pageManager.EditTemplate(pageTemplate.Id);
                    var temp = pageManager.TemplatesLifecycle.CheckOut(master);

                    if (temp != null)
                    {
                        master = pageManager.TemplatesLifecycle.CheckIn(temp);
                        master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        pageManager.TemplatesLifecycle.Publish(master);
                        pageManager.SaveChanges();
                        result = true;
                    }
                }
            }

            return result;
        }

        public static bool CreateBlog(Guid blogId, string title, string description)
        {
            bool result = false;
            using (var fluent = App.WorkWith())
            {
                var blogsFacade = fluent.Blogs();
                using (new ElevatedModeRegion(blogsFacade.GetManager()))
                {
                    var blog = blogsFacade.Where(b => b.Title == title).Get().FirstOrDefault();

                    if (blog == null)
                    {
                        var blogFacade = fluent.Blog();
                        using (new ElevatedModeRegion(blogFacade.GetManager()))
                        {
                            blogFacade.CreateNew(blogId).Do(b =>
                            {
                                b.Title = title;
                                b.Description = description;
                            })
                            .SaveChanges();
                        }
                        result = true;
                    }
                }
            }
            return result;
        }

        public static bool CreateLocalizedBlog(Guid blogId, string title, string description, List<string> cultures)
        {
            bool result = false;
            using (var fluent = App.WorkWith())
            {
                var blogsFacade = fluent.Blogs();
                using (new ElevatedModeRegion(blogsFacade.GetManager()))
                {
                    var count = 0;
                    blogsFacade.Where(b => b.Id == blogId).Count(out count);

                    if (count == 0)
                    {
                        bool blogCreated = false;

                        foreach (string culture in cultures)
                        {
                            var cultureInfo = new CultureInfo(culture);
                            var currentCulture = Thread.CurrentThread.CurrentUICulture;
                            Thread.CurrentThread.CurrentUICulture = cultureInfo;

                            if (!blogCreated)
                            {
                                var blogFacade = fluent.Blog();
                                using (new ElevatedModeRegion(blogFacade.GetManager()))
                                {
                                    blogFacade.CreateNew(blogId).Do(b =>
                                        {
                                            b.Title[cultureInfo] = title;
                                            b.Description[cultureInfo] = description;
                                            b.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                            blogCreated = true;
                                        }).SaveChanges();

                                    result = true;
                                }
                            }
                            else
                            {
                                var blogFacade = fluent.Blog(blogId);
                                using (new ElevatedModeRegion(blogFacade.GetManager()))
                                {
                                    blogFacade.Do(b =>
                                    {
                                        b.Title[cultureInfo] = title;
                                        b.Description[cultureInfo] = description;
                                        b.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                    }).SaveChanges();
                                }
                                result = true;
                            }

                            Thread.CurrentThread.CurrentUICulture = currentCulture;
                        }
                    }
                }
            }
            return result;
        }

        public static void CreateBlogPost(Guid blogId, string title, string content, string author, string summary)
        {
            var blogsManager = BlogsManager.GetManager();
            using (new ElevatedModeRegion(blogsManager))
            {
                var blog = blogsManager.GetBlogs().Where(b => b.Id == blogId).SingleOrDefault();
                if (blog != null)
                {
                    var post = blogsManager.CreateBlogPost();
                    post.Parent = blog;
                    post.Summary = summary;
                    post.Title = title;
                    post.Content = content;
                    post.DateCreated = DateTime.Today;
                    post.PublicationDate = DateTime.UtcNow;
                    post.ExpirationDate = DateTime.Today.AddDays(365);
                    post.UrlName = Regex.Replace(post.Title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                    post.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                    blogsManager.RecompileItemUrls<BlogPost>(post);
                    blogsManager.SaveChanges();

                    var master = blogsManager.Lifecycle.CheckOut(post);
                    master = blogsManager.Lifecycle.CheckIn(master);
                    blogsManager.Lifecycle.Publish(master);
                    blogsManager.SaveChanges();
                }
            }
        }

        public static void CreateLocalizedBlogPost(Guid blogPostId, Guid parentBlogId, string title, string content, string summary, Guid ownerId, List<string> tags, List<string> categories, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            using (var fluent = App.WorkWith())
            {
                var blogsFacade = fluent.Blogs();
                using (new ElevatedModeRegion(blogsFacade.GetManager()))
                {
                    var blogCount = 0;
                    blogsFacade.Where(b => b.Id == parentBlogId).Count(out blogCount);
                    if (blogCount > 0)
                    {
                        var blogPostsFacade = fluent.BlogPosts();
                        using (new ElevatedModeRegion(blogPostsFacade.GetManager()))
                        {
                            var blogPostCount = 0;
                            blogPostsFacade.Where(p => p.Id == blogPostId).Count(out blogPostCount);

                            if (blogPostCount == 0)
                            {
                                var blogFacade = fluent.Blog(parentBlogId);
                                using (new ElevatedModeRegion(blogFacade.GetManager()))
                                {
                                    blogFacade.CreateBlogPost(blogPostId).Do(blogPost =>
                                    {
                                        blogPost.Owner = ownerId;
                                        blogPost.Urls.Clear();
                                        blogPost.DateCreated = DateTime.UtcNow;
                                        blogPost.PublicationDate = DateTime.UtcNow;
                                        blogPost.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                    }).CheckOut().CheckInAndPublish().SaveChanges();
                                }
                            }

                            var blogPostFacade = fluent.BlogPost(blogPostId);
                            using (new ElevatedModeRegion(blogPostFacade.GetManager()))
                            {
                                blogPostFacade.CheckOut().Do(blogPost =>
                                {
                                    blogPost.Title[cultureInfo] = title;
                                    blogPost.GetString("Content")[cultureInfo] = content;
                                    blogPost.Summary[cultureInfo] = summary;
                                    blogPost.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);

                                    if (categories != null && categories.Count > 0)
                                    {
                                        foreach (string category in categories)
                                        {
                                            var taxManager = TaxonomyManager.GetManager();
                                            using (new ElevatedModeRegion(taxManager))
                                            {
                                                var taxon = taxManager.GetTaxa<HierarchicalTaxon>().Where(t => t.Name == category).SingleOrDefault();
                                                if (taxon != null)
                                                {
                                                    blogPost.Organizer.AddTaxa("Category", taxon.Id);
                                                }
                                            }
                                        }
                                    }

                                    if (tags != null && tags.Count > 0)
                                    {
                                        foreach (string tag in tags)
                                        {
                                            var taxManager = TaxonomyManager.GetManager();
                                            using (new ElevatedModeRegion(taxManager))
                                            {
                                                var taxon = taxManager.GetTaxa<FlatTaxon>().Where(t => t.Name == tag).SingleOrDefault();
                                                if (taxon != null)
                                                {
                                                    blogPost.Organizer.AddTaxa("Tags", taxon.Id);
                                                }
                                            }
                                        }
                                    }

                                    blogPost.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                                }).CheckIn().Publish().SaveChanges();
                            }
                        }
                    }
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;
        }

        public static IComment CreateBlogPostComment(Guid blogPostId, string message, IAuthor author, string website, string ip)
        {
            var blogsManager = BlogsManager.GetManager();
            var blogPost = blogsManager.GetBlogPost(blogPostId);
            if (blogPost != null)
            {
                var cs = SystemManager.GetCommentsService();

                var threadReg = new CreateThreadRegion(cs, key: blogPostId.ToString());
                var commentProxy = new CommentProxy(message, threadReg.Thread.Key, author, ip);
                var comment = cs.CreateComment(commentProxy);
                return comment;
            }
            return null;
        }

        public static void CreateCampaign(Guid id, Guid bodyId, string name, string fromName, string subject, string replyToMail, bool useGoogleTracking, CampaignState state)
        {
            NewslettersManager manager = NewslettersManager.GetManager();
            using (new ElevatedModeRegion(manager))
            {
                Campaign cam = manager.GetCampaigns().Where(c => c.Id == id).SingleOrDefault();

                if (cam == null)
                {
                    cam = manager.CreateCampaign(false, id);
                    cam.MessageBody = manager.GetMessageBody(bodyId);
                    cam.Name = name;
                    cam.FromName = fromName;
                    cam.MessageSubject = subject;
                    cam.ReplyToEmail = replyToMail;
                    cam.UseGoogleTracking = useGoogleTracking;
                    cam.CampaignState = state;
                    manager.SaveChanges();
                }
            }
        }

        public static void CreateCategory(string title)
        {
            CreateCategory(title, string.Empty);
        }

        public static void CreateCategory(string title, string parentTitle)
        {
            var manager = TaxonomyManager.GetManager();
            using (new ElevatedModeRegion(manager))
            {
                var categories = manager.GetTaxonomies<HierarchicalTaxonomy>().Where(t => t.Name == "Categories").SingleOrDefault();

                if (categories != null)
                {
                    var category = categories.Taxa.Where(t => t.Name == title).SingleOrDefault() as HierarchicalTaxon;

                    if (category == null)
                    {
                        category = manager.CreateTaxon<HierarchicalTaxon>();
                        category.Title = title;
                        category.Name = title;
                        category.UrlName = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                    }

                    if (String.IsNullOrEmpty(parentTitle))
                    {
                        categories.Taxa.Add(category);
                    }
                    else
                    {
                        var parentCategory = categories.Taxa.Where(t => t.Name == parentTitle).SingleOrDefault() as HierarchicalTaxon;

                        if (parentCategory != null)
                        {
                            category.Taxonomy = categories;
                            category.Parent = parentCategory;
                            parentCategory.Subtaxa.Add(category);
                        }
                    }

                    manager.SaveChanges();
                }
            }
        }

        public static void CreateContentFeed(string feedTitle, Guid backLinksPageId, Type contentType, int maxItems, RssContentOutputSetting outputSettings, RssFormatOutputSettings formatSettings)
        {
            PublishingManager manager = PublishingManager.GetManager();
            using (new ElevatedModeRegion(manager))
            {
                if (manager.GetPublishingPoints().Where(p => p.Name == feedTitle).Count() == 0)
                {
                    var publishingPoint = manager.CreatePublishingPoint();
                    publishingPoint.Name = feedTitle;
                    publishingPoint.IsActive = true;
                    publishingPoint.PublishingPointBusinessObjectName = "PassThrough";

                    List<Mapping> inboundMappings = new List<Mapping>();
                    inboundMappings.Add(CreateMapping(PublishingConstants.FieldContent, PublishingConstants.FieldContent));
                    inboundMappings.Add(CreateMapping(PublishingConstants.FieldLink, PublishingConstants.FieldLink));
                    inboundMappings.Add(CreateMapping(PublishingConstants.FieldPublicationDate, PublishingConstants.FieldPublicationDate));
                    inboundMappings.Add(CreateMapping(PublishingConstants.FieldTitle, PublishingConstants.FieldTitle));

                    inboundMappings.Add(CreateMapping(PublishingConstants.FieldContentType, PublishingConstants.FieldContentType));
                    inboundMappings.Add(CreateMapping(PublishingConstants.FieldItemId, PublishingConstants.FieldOriginalContentId));
                    inboundMappings.Add(CreateMapping(PublishingConstants.FieldPipeId, PublishingConstants.FieldPipeId));

                    var contentInboundPipeSettings = CreateSitefinityContentPipeSettings(publishingPoint, contentType, inboundMappings, true, ContentInboundPipe.PipeName, true, PipeInvokationMode.Push, backLinksPageId);

                    publishingPoint.PipeSettings.Add(contentInboundPipeSettings);

                    List<Mapping> outboundMappings = new List<Mapping>();
                    outboundMappings.Add(CreateMapping(PublishingConstants.FieldContent, PublishingConstants.FieldContent, true));
                    outboundMappings.Add(CreateMapping(PublishingConstants.FieldLink, PublishingConstants.FieldLink, true));
                    outboundMappings.Add(CreateMapping(PublishingConstants.FieldPublicationDate, PublishingConstants.FieldPublicationDate, false));
                    outboundMappings.Add(CreateMapping(PublishingConstants.FieldTitle, PublishingConstants.FieldTitle, true));

                    var rssOutboundPipeSettings = CreateRssPipeSettings(publishingPoint, outboundMappings, false, RSSOutboundPipe.PipeName, true, PipeInvokationMode.Pull, outputSettings, formatSettings, maxItems, 400);

                    publishingPoint.PipeSettings.Add(rssOutboundPipeSettings);

                    List<SimpleDefinitionField> fields = GetDefaultSimpleDefinitionFields();
                    PublishingPointFactory.CreatePublishingPointDataItem(fields, publishingPoint);

                    MetadataManager.GetManager().SaveChanges(true);

                    manager.SaveChanges();
                }
            }
        }

        public static void CreateEvent(string title, string content, DateTime startDate, DateTime endDate)
        {
            CreateEvent(title, content, startDate, endDate, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        public static void CreateEvent(string title, string content, DateTime startDate, DateTime endDate, string street, string city, string state, string country)
        {
            CreateEvent(title, content, startDate, endDate, street, city, state, country, string.Empty, string.Empty, string.Empty);
        }

        public static void CreateEvent(string title, string content, DateTime startDate, DateTime endDate, string street, string city, string state, string country, string contatcEmail, string contactWeb, string contactName)
        {
            var eventId = Guid.Empty;

            using (var fluent = App.WorkWith())
            {
                var eventFacade = fluent.Event();
                using (new ElevatedModeRegion(eventFacade.GetManager()))
                {
                    eventFacade.CreateNew()
                        .Do(e =>
                        {
                            eventId = e.Id;
                            e.Title = title;
                            e.Content = content;
                            e.EventStart = startDate;
                            e.EventEnd = endDate;
                            e.Street = street;
                            e.City = city;
                            e.State = state;
                            e.Country = country;
                            e.ContactEmail = contatcEmail;
                            e.ContactName = contactName;
                            e.ContactWeb = contactWeb;

                            e.PublicationDate = DateTime.Today;
                            e.ExpirationDate = DateTime.Today.AddDays(365);
                            e.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        }).Publish().SaveChanges();
                }
            }
        }

        public static void CreateLocalizedEvent(Guid id, string title, string content, string summary, DateTime eventStart, DateTime eventEnd, string street, string city, string country, string state, string email, string website, string contactName, string cellPhone, string phone, string culture = "en")
        {
            CreateLocalizedEvent(id, title, content, summary, eventStart, eventEnd, street, city, country, state, email, website, contactName, cellPhone, phone, culture);
        }

        public static void CreateLocalizedEvent(Guid id, string title, string content, string summary, DateTime eventStart, DateTime eventEnd, string street, string city, string country, string state, string email, string website, string contactName, string cellPhone, string phone, List<string> tags, List<string> categories, string culture = "en")
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            using (var fluent = App.WorkWith())
            {
                var eventsFacade = fluent.Events();
                using (new ElevatedModeRegion(eventsFacade.GetManager()))
                {
                    int count;
                    eventsFacade.Where(e => e.Id == id).Count(out count);

                    if (count == 0)
                    {
                        var eventFacadeNew = fluent.Event();
                        using (new ElevatedModeRegion(eventFacadeNew.GetManager()))
                        {
                            eventFacadeNew.CreateNew(id).Do(eventItem =>
                            {
                                eventItem.PublicationDate = DateTime.Today;
                                eventItem.EventStart = eventStart;
                                eventItem.EventEnd = eventEnd;
                                eventItem.ContactEmail = email;
                                eventItem.ContactWeb = website;
                                eventItem.Urls.Clear();
                                eventItem.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                eventItem.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                            }).Publish().SaveChanges();
                        }
                    }

                    var eventFacade = fluent.Event(id);
                    using (new ElevatedModeRegion(eventFacade.GetManager()))
                    {
                        eventFacade.CheckOut().Do(eventItem =>
                        {
                            eventItem.Title[cultureInfo] = title;
                            eventItem.GetString("Content")[cultureInfo] = content;
                            eventItem.Summary[cultureInfo] = summary;
                            eventItem.Street[cultureInfo] = street;
                            eventItem.City[cultureInfo] = city;
                            eventItem.Country[cultureInfo] = country;
                            eventItem.State[cultureInfo] = state;
                            eventItem.ContactName[cultureInfo] = contactName;
                            eventItem.ContactCell[cultureInfo] = cellPhone;
                            eventItem.ContactPhone[cultureInfo] = phone;
                            eventItem.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);

                            if (categories != null && categories.Count > 0)
                            {
                                foreach (string category in categories)
                                {
                                    var taxManager = TaxonomyManager.GetManager();
                                    using (new ElevatedModeRegion(taxManager))
                                    {
                                        var taxon = taxManager.GetTaxa<HierarchicalTaxon>().Where(t => t.Name == category).SingleOrDefault();
                                        if (taxon != null)
                                        {
                                            eventItem.Organizer.AddTaxa("Category", taxon.Id);
                                        }
                                    }
                                }
                            }

                            if (tags != null && tags.Count > 0)
                            {
                                foreach (string tag in tags)
                                {
                                    var taxManager = TaxonomyManager.GetManager();
                                    using (new ElevatedModeRegion(taxManager))
                                    {
                                        var taxon = taxManager.GetTaxa<FlatTaxon>().Where(t => t.Name == tag).SingleOrDefault();
                                        if (taxon != null)
                                        {
                                            eventItem.Organizer.AddTaxa("Tags", taxon.Id);
                                        }
                                    }
                                }
                            }
                            eventItem.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        }).CheckIn().Publish().SaveChanges();
                    }
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;
        }

        public static void CreateForm(Guid formId, string formName, string formTitle, string formSuccessMessage, Dictionary<Control, string> formControls)
        {
            var formManager = FormsManager.GetManager();
            using (new ElevatedModeRegion(formManager))
            {
                var form = formManager.GetForms().SingleOrDefault(f => f.Id == formId);
                Guid siblingId = Guid.Empty;

                if (form == null)
                {
                    form = formManager.CreateForm(formName, formId);

                    form.Title = formTitle;
                    form.UrlName = Regex.Replace(form.Name.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                    form.SuccessMessage = formSuccessMessage;

                    var master = formManager.EditForm(form.Id);
                    var temp = formManager.Lifecycle.CheckOut(master);

                    if (temp != null)
                    {
                        if (formControls != null && formControls.Count > 0)
                        {
                            int controlsCounter = 0;
                            foreach (var control in formControls)
                            {
                                control.Key.ID = string.Format(formName + "_C" + controlsCounter.ToString().PadLeft(3, '0'));
                                controlsCounter++;
                                var formControl = formManager.CreateControl<FormDraftControl>(control.Key, control.Value);

                                formControl.SiblingId = siblingId;
                                formControl.Caption = control.Key.GetType().Name;
                                siblingId = formControl.Id;

                                temp.Controls.Add(formControl);
                            }
                        }

                        master = formManager.Lifecycle.CheckIn(temp);
                        master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        formManager.Lifecycle.Publish(master);

                        formManager.SaveChanges(true);
                    }
                }
            }
        }

        public static void CreateLocalizedForm(Guid formId, string formName, string formTitle, string culture)
        {
            CreateLocalizedForm(formId, formName, formTitle, string.Empty, culture);
        }

        public static bool CreateLocalizedForm(Guid formId, string formName, string formTitle, string formSuccessMessage, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            bool result = false;

            var formManager = FormsManager.GetManager();
            using (new ElevatedModeRegion(formManager))
            {
                var form = formManager.GetForms().Where(f => f.Id == formId).SingleOrDefault();

                bool isBeingCreated = false;

                if (form == null)
                {
                    form = formManager.CreateForm(formName, formId);
                    isBeingCreated = true;
                }

                if (isBeingCreated || form.AvailableCultures.Where(c => c.Name == cultureInfo.Name).Count() == 0)
                {
                    form.UrlName[cultureInfo] = Regex.Replace(form.Name.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                    form.Title[cultureInfo] = formTitle;
                    form.SuccessMessage[cultureInfo] = formSuccessMessage;
                    form.SubmitAction = SubmitAction.TextMessage;

                    var master = formManager.Lifecycle.Edit(form, cultureInfo);
                    var temp = formManager.Lifecycle.CheckOut(master, cultureInfo);

                    master = formManager.Lifecycle.CheckIn(temp, cultureInfo);
                    master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                    formManager.Lifecycle.Publish(master, cultureInfo);
                    formManager.SaveChanges(true);

                    result = true;
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;

            return result;
        }

        public static bool CreateLocalizedList(Guid listId, string title, string description, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            bool result = false;

            using (var fluent = App.WorkWith())
            {
                var listsFacade = fluent.Lists();
                using (new ElevatedModeRegion(listsFacade.GetManager()))
                {
                    int count;
                    listsFacade.Where(l => l.Id == listId).Count(out count);

                    if (count == 0)
                    {
                        var listFacadeNew = fluent.List();
                        using (new ElevatedModeRegion(listFacadeNew.GetManager()))
                        {
                            listFacadeNew.CreateNew(listId).Do(l =>
                            {
                                l.Title[cultureInfo] = title;
                                l.Description[cultureInfo] = description;
                                l.Urls.Clear();
                                l.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                            }).SaveChanges();
                        }
                        result = true;
                    }
                    else
                    {
                        var listFacadeNewId = fluent.List(listId);
                        using (new ElevatedModeRegion(listFacadeNewId.GetManager()))
                        {
                            listFacadeNewId.Do(l =>
                            {
                                if (l.AvailableCultures.Where(c => c.Name == cultureInfo.Name).Count() == 0)
                                {
                                    l.Title[cultureInfo] = title;
                                    l.Description[cultureInfo] = description;
                                    l.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                }
                            }).SaveChanges();
                        }
                    }
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;

            return result;
        }

        public static Guid CreateLocalizedListItem(Guid listItemId, string title, string content, string culture)
        {
            return CreateLocalizedListItem(listItemId, Guid.Empty, title, content, Guid.Empty, culture);
        }

        public static Guid CreateLocalizedListItem(Guid listItemId, Guid parentListId, string title, string content, Guid owner, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            Guid itemId = Guid.Empty;

            using (var fluent = App.WorkWith())
            {
                var listItemsFacade = fluent.ListItems();
                using (new ElevatedModeRegion(listItemsFacade.GetManager()))
                {
                    int listItemCount;
                    listItemsFacade.Where(l => l.Id == listItemId).Count(out listItemCount);

                    if (listItemCount == 0)
                    {
                        var listsFacade = fluent.Lists();
                        using (new ElevatedModeRegion(listsFacade.GetManager()))
                        {
                            int listCount;
                            listsFacade.Where(l => l.Id == parentListId).Count(out listCount);

                            if (listCount > 0)
                            {
                                var listFacadeId = fluent.List(parentListId);
                                using (new ElevatedModeRegion(listFacadeId.GetManager()))
                                {
                                    listFacadeId.CreateListItem().Do(listItem =>
                                    {
                                        itemId = listItem.Id;
                                        listItem.Owner = owner;
                                        listItem.DateCreated = DateTime.UtcNow;
                                        listItem.PublicationDate = DateTime.UtcNow;
                                        listItem.Urls.Clear();
                                        listItem.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                        listItem.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                                    }).Publish().SaveChanges();
                                }
                            }
                        }
                    }

                    if (itemId == Guid.Empty)
                    {
                        itemId = listItemId;
                    }
                    var listItemFacadeId = fluent.ListItem(itemId);
                    using (new ElevatedModeRegion(listItemFacadeId.GetManager()))
                    {
                        listItemFacadeId.CheckOut().Do(listItem =>
                        {
                            listItem.Title[cultureInfo] = title;
                            listItem.GetString("Content")[cultureInfo] = content;
                            listItem.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                            listItem.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        }).CheckIn().Publish().SaveChanges();
                    }
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;
            return itemId;
        }

        public static void CreateNewsItem(string newsTitle, string newsContent)
        {
            CreateNewsItem(newsTitle, newsContent, string.Empty, string.Empty);
        }

        public static void CreateNewsItem(string newsTitle, string newsDescription, string newsContent)
        {
            CreateNewsItem(newsTitle, newsDescription, newsContent, string.Empty, string.Empty);
        }

        public static void CreateNewsItem(string newsTitle, string newsContent, string summary, string author)
        {
            CreateNewsItem(newsTitle, string.Empty, newsContent, summary, author);
        }

        public static void CreateNewsItem(string newsTitle, string newsDescription, string newsContent, string summary, string author)
        {
            var newsId = Guid.Empty;

            using (var fluent = App.WorkWith())
            {
                var newsItemNewFacade = fluent.NewsItem();
                using (new ElevatedModeRegion(newsItemNewFacade.GetManager()))
                {
                    newsItemNewFacade.CreateNew().Do(nI =>
                    {
                        newsId = nI.Id;
                        nI.Title = newsTitle;
                        nI.Description = newsDescription;
                        nI.DateCreated = DateTime.UtcNow;
                        nI.PublicationDate = DateTime.UtcNow.AddDays(1);
                        nI.ExpirationDate = DateTime.UtcNow.AddYears(1);
                        nI.Content = newsContent;
                        nI.Summary = summary;
                        nI.Author = author;
                        nI.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                    }).Publish().SaveChanges();
                }
            }
        }

        public static void CreateLocalizedNewsItem(Guid newsId, string newsTitle, string newsContent, string culture)
        {
            CreateLocalizedNewsItem(newsId, newsTitle, newsContent, string.Empty, string.Empty, culture);
        }

        public static void CreateLocalizedNewsItem(Guid newsId, string newsTitle, string newsContent, List<string> tags, string culture)
        {
            CreateLocalizedNewsItem(newsId, newsTitle, newsContent, string.Empty, string.Empty, tags, null, culture);
        }

        public static void CreateLocalizedNewsItem(Guid newsId, string newsTitle, string newsContent, List<string> tags, List<string> categories, string culture)
        {
            CreateLocalizedNewsItem(newsId, newsTitle, newsContent, string.Empty, string.Empty, tags, categories, culture);
        }

        public static void CreateLocalizedNewsItem(Guid newsId, string newsTitle, string newsContent, string summary, string author, string culture)
        {
            CreateLocalizedNewsItem(newsId, newsTitle, newsContent, summary, author, string.Empty, string.Empty, null, null, culture);
        }

        public static void CreateLocalizedNewsItem(Guid newsId, string newsTitle, string newsContent, string summary, string author, string sourceName, string sourceUrl, string culture)
        {
            CreateLocalizedNewsItem(newsId, newsTitle, newsContent, summary, author, sourceName, sourceUrl, null, null, culture);
        }

        public static void CreateLocalizedNewsItem(Guid newsId, string newsTitle, string newsContent, string summary, string author, List<string> tags, List<string> categories, string culture)
        {
            CreateLocalizedNewsItem(newsId, newsTitle, newsContent, summary, author, string.Empty, string.Empty, tags, categories, culture);
        }

        public static void CreateLocalizedNewsItem(Guid newsId, string newsTitle, string newsContent, string summary, string author, string sourceName, string sourceUrl, List<string> tags, List<string> categories, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            using (var fluent = App.WorkWith())
            {
                var newsItemsFacade = fluent.NewsItems();
                using (new ElevatedModeRegion(newsItemsFacade.GetManager()))
                {
                    int count;
                    newsItemsFacade.Where(n => n.Id == newsId).Count(out count);

                    if (count == 0)
                    {
                        var newsItemNewFacade = fluent.NewsItem();
                        using (new ElevatedModeRegion(newsItemNewFacade.GetManager()))
                        {
                            newsItemNewFacade.CreateNew(newsId).Do(item =>
                            {
                                item.DateCreated = DateTime.UtcNow;
                                item.PublicationDate = DateTime.UtcNow;
                                item.SourceName = sourceName;
                                item.SourceSite = sourceUrl;
                                item.UrlName[cultureInfo] = Regex.Replace(newsTitle.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                item.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                            }).Publish().SaveChanges();
                        }
                    }

                    var newsItemNewIdFacade = fluent.NewsItem(newsId);
                    using (new ElevatedModeRegion(newsItemNewIdFacade.GetManager()))
                    {
                        newsItemNewIdFacade.CheckOut().Do(item =>
                            {
                                item.Title[cultureInfo] = newsTitle;
                                item.GetString("Content")[cultureInfo] = newsContent;
                                item.Summary[cultureInfo] = summary;
                                item.Author[cultureInfo] = author;
                                item.UrlName[cultureInfo] = Regex.Replace(newsTitle.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);

                                if (categories != null && categories.Count > 0)
                                {
                                    foreach (string category in categories)
                                    {
                                        var taxManager = TaxonomyManager.GetManager();
                                        using (new ElevatedModeRegion(taxManager))
                                        {
                                            var taxon = taxManager.GetTaxa<HierarchicalTaxon>().Where(t => t.Name == category).SingleOrDefault();
                                            if (taxon != null)
                                            {
                                                item.Organizer.AddTaxa("Category", taxon.Id);
                                            }
                                        }
                                    }
                                }

                                if (tags != null && tags.Count > 0)
                                {
                                    foreach (string tag in tags)
                                    {
                                        var taxManager = TaxonomyManager.GetManager();
                                        using (new ElevatedModeRegion(taxManager))
                                        {
                                            var taxon = taxManager.GetTaxa<FlatTaxon>().Where(t => t.Name == tag).SingleOrDefault();
                                            if (taxon != null)
                                            {
                                                item.Organizer.AddTaxa("Tags", taxon.Id);
                                            }
                                        }
                                    }
                                }
                                item.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                            }).CheckIn().Publish().SaveChanges();
                    }
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;
        }

        public static IComment CreateNewsItemComment(Guid newsItemId, string message, IAuthor author, string ip)
        {
            var comment = CreateNewsCommentNativeAPI(newsItemId, message, author, ip);
            return comment;
        }

        public static IComment CreateNewsCommentNativeAPI(Guid masterNewsItemId, string message, IAuthor author, string ip)
        {
            NewsManager manager = NewsManager.GetManager();
            using (new ElevatedModeRegion(manager))
            {
                NewsItem newsItem = manager.GetNewsItems().Where(nI => nI.Id == masterNewsItemId).FirstOrDefault();

                if (newsItem != null)
                {
                    var cs = SystemManager.GetCommentsService();

                    var threadReg = new CreateThreadRegion(cs, key: masterNewsItemId.ToString());
                    var commentProxy = new CommentProxy(message, threadReg.Thread.Key, author, ip);
                    var comment = cs.CreateComment(commentProxy);
                    return comment;
                }
                return null;
            }
        }

        public static bool CreatePage(string pageName)
        {
            return CreatePage(Guid.Empty, pageName);
        }

        public static bool CreatePage(Guid pageId, string pageName)
        {
            return CreatePage(pageId, pageName, Guid.Empty);
        }

        public static bool CreatePage(Guid pageId, string pageName, bool isHomePage)
        {
            return CreatePage(pageId, pageName, isHomePage, Guid.Empty);
        }

        public static bool CreatePage(Guid pageId, string pageName, Guid parentPageId)
        {
            return CreatePage(pageId, pageName, false, parentPageId);
        }

        public static bool CreatePage(Guid pageId, string pageName, bool isHomePage, Guid parentPageId)
        {
            bool result = false;
            var pageDataId = Guid.NewGuid();
            var parentId = parentPageId;

            if (parentId == Guid.Empty)
            {
                parentId = SiteInitializer.CurrentFrontendRootNodeId;
            }

            using (var fluent = App.WorkWith())
            {
                var pagesFacade = fluent.Pages();
                using (new ElevatedModeRegion(pagesFacade.GetManager()))
                {
                    int count;
                    pagesFacade.Where(p => p.Id == pageId).Count(out count);

                    if (count == 0)
                    {
                        var pageFacade = fluent.Page();
                        pageFacade
                        .CreateNewStandardPage(parentId, pageId, pageDataId)
                        .Do(p =>
                        {
                            p.Title = pageName;
                            p.Name = pageName;
                            p.Description = pageName;
                            p.UrlName = Regex.Replace(pageName.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                            p.ShowInNavigation = true;
                            p.DateCreated = DateTime.UtcNow;
                            p.LastModified = DateTime.UtcNow;
                            p.Page.HtmlTitle = pageName;

                            p.Page.HtmlTitle = pageName;
                            p.Page.Title = pageName;
                            p.Page.Description = pageName;
                            p.Page.Culture = Thread.CurrentThread.CurrentCulture.ToString();
                            p.Page.UiCulture = Thread.CurrentThread.CurrentUICulture.ToString();
                            p.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        }).CheckOut().Publish().SaveChanges();

                        if (isHomePage)
                        {
                            SystemManager.CurrentContext.CurrentSite.SetHomePage(pageId);
                        }

                        result = true;
                    }
                }
            }

            return result;
        }

        public static bool CreateLocalizedPage(Guid pageId, string pageName, string culture = "en")
        {
            return CreateLocalizedPage(pageId, pageName, Guid.Empty, false, true, culture);
        }

        public static bool CreateLocalizedPage(Guid pageId, string pageName, bool isHomePage, string culture = "en")
        {
            return CreateLocalizedPage(pageId, pageName, Guid.Empty, isHomePage, true, culture);
        }

        public static bool CreateLocalizedPage(Guid pageId, string pageName, Guid parentId, string culture = "en")
        {
            return CreateLocalizedPage(pageId, pageName, parentId, false, true, culture);
        }

        public static bool CreateLocalizedPage(Guid pageId, string pageName, bool isHomePage, bool showInNavigation, string culture = "en")
        {
            return CreateLocalizedPage(pageId, pageName, Guid.Empty, isHomePage, showInNavigation, culture);
        }

        public static bool CreateLocalizedPage(Guid pageId, string pageName, Guid parentPageId, bool isHomePage, bool showInNavigation, string culture = "en")
        {
            //The CurrentUICulture must be set to the desired culture for the page while translating it.
            //At the end of the method the CurrentUICulture is restored to its original value.
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var result = false;
            var pageDataId = Guid.NewGuid();
            using (PageManager pageManager = PageManager.GetManager())
            {
                using (new ElevatedModeRegion(pageManager))
                {
                    var initialPageNode = pageManager.GetPageNodes().Where(n => n.Id == pageId).SingleOrDefault();

                    if (initialPageNode != null && LanguageExistsForPage(initialPageNode.Page, cultureInfo))
                    {
                        return result;
                    }

                    result = true;

                    PageData pageData;
                    PageNode pageNode;
                    if (initialPageNode == null)
                    {
                        var parentId = parentPageId;
                        if (parentId == Guid.Empty)
                        {
                            parentId = SiteInitializer.CurrentFrontendRootNodeId;
                        }
                        //Create Page
                        PageNode parent = pageManager.GetPageNode(parentId);
                        pageNode = pageManager.CreatePage(parent, pageId, NodeType.Standard);

                        pageData = pageManager.CreatePageData(pageDataId);
                        pageData.Culture = Thread.CurrentThread.CurrentCulture.ToString();
                        pageData.UiCulture = Thread.CurrentThread.CurrentUICulture.ToString();

                        pageNode.Page = pageData;
                        pageNode.Name = pageName;
                        pageNode.ShowInNavigation = true;
                        pageNode.DateCreated = DateTime.UtcNow;
                        pageNode.LastModified = DateTime.UtcNow;
                    }
                    else
                    {
                        //TranslatePage
                        pageManager.InitializePageLocalizationStrategy(initialPageNode, LocalizationStrategy.Split, false);
                        pageNode = GetPageNodeForLanguage(initialPageNode.Page, cultureInfo);
                        pageData = pageNode.Page;
                        pageData.TranslationInitialized = true;
                        pageData.IsAutoCreated = false;
                    }
                    pageNode.UrlName[cultureInfo] = Regex.Replace(pageName.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                    pageNode.Description[cultureInfo] = pageName;
                    pageNode.Title[cultureInfo] = pageName;
                    pageNode.ShowInNavigation = showInNavigation;

                    pageData.HtmlTitle[cultureInfo] = pageName;
                    pageData.Title[cultureInfo] = pageName;
                    pageData.Description[cultureInfo] = pageName;

                    pageNode.ApprovalWorkflowState = SampleUtilities.ApprovalWorkflowStatePublished;

                    pageManager.SaveChanges();
                    SystemManager.CurrentContext.CurrentSite.SetHomePage(pageId);
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;

            return result;
        }

        public static bool CreatePageGroup(Guid pageGroupId, Guid parentPageId, string pageGroupTitle, string culture)
        {
            bool result = true;
            var parentId = parentPageId;

            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            if (parentId == Guid.Empty)
            {
                parentId = SiteInitializer.CurrentFrontendRootNodeId;
            }

            using (var fluent = App.WorkWith())
            {
                var pagesFacade = fluent.Pages();
                using (new ElevatedModeRegion(pagesFacade.GetManager()))
                {
                    var count = 0;
                    pagesFacade.Where(p => p.Id == pageGroupId).Count(out count);

                    if (count == 0)
                    {
                        var pageFacade = fluent.Page();
                        using (new ElevatedModeRegion(pagesFacade.GetManager()))
                        {
                            pageFacade.CreateNewPageGroup(parentId, pageGroupId).Do(pN =>
                            {
                                pN.Name = pageGroupTitle;
                                pN.ShowInNavigation = false;
                                pN.DateCreated = DateTime.UtcNow;
                                pN.LastModified = DateTime.UtcNow;
                                pN.UrlName[cultureInfo] = Regex.Replace(pageGroupTitle.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                            }).SaveChanges();
                        }
                    }
                    else
                    {
                        var pageGroupFacade = fluent.Page(pageGroupId);
                        using (new ElevatedModeRegion(pageGroupFacade.GetManager()))
                        {
                            var pageGroup = pageGroupFacade.Get();
                            if (pageGroup.AvailableCultures.Contains(cultureInfo))
                            {
                                result = false;
                            }
                        }
                    }

                    if (result)
                    {
                        var pageGroupIdFacade = fluent.Page(pageGroupId);
                        using (new ElevatedModeRegion(pageGroupIdFacade.GetManager()))
                        {
                            pageGroupIdFacade.Do(pN =>
                            {
                                pN.Title[cultureInfo] = pageGroupTitle;
                                pN.Description[cultureInfo] = pageGroupTitle;
                                pN.UrlName[cultureInfo] = Regex.Replace(pageGroupTitle.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                            }).SaveChanges();
                        }
                    }
                }
            }

            Thread.CurrentThread.CurrentUICulture = currentCulture;

            return result;
        }

        public static void CreateMailingList(Guid id, string title, string fromName, string replyToEmail, string subject)
        {
            NewslettersManager manager = NewslettersManager.GetManager();
            using (new ElevatedModeRegion(manager))
            {
                MailingList mailList = manager.GetMailingLists().Where(l => l.Id == id).SingleOrDefault();

                if (mailList == null)
                {
                    mailList = manager.Provider.CreateList(id);
                    mailList.Title = title;
                    mailList.DefaultFromName = fromName;
                    mailList.DefaultReplyToEmail = replyToEmail;
                    mailList.DefaultSubject = subject;
                    manager.SaveChanges();
                }
            }
        }

        public static void CreateMessageBody(Guid id, MessageBodyType bodyType, string bodyText, string name)
        {
            NewslettersManager manager = NewslettersManager.GetManager();
            using (new ElevatedModeRegion(manager))
            {
                MessageBody messageBody = manager.GetMessageBodies().Where(b => b.Id == id).SingleOrDefault();

                if (messageBody == null)
                {
                    messageBody = manager.CreateMessageBody(id);
                    messageBody.MessageBodyType = bodyType;
                    messageBody.BodyText = bodyText;
                    messageBody.Name = name;
                    messageBody.IsTemplate = true;
                    manager.SaveChanges();
                }
            }
        }

        public static void CreateTag(string title, Guid tagId)
        {
            var manager = TaxonomyManager.GetManager();
            using (new ElevatedModeRegion(manager))
            {
                var tags = manager.GetTaxonomies<FlatTaxonomy>().Where(t => t.Name == "Tags").SingleOrDefault();
                if (tags != null)
                {
                    FlatTaxon taxon = manager.GetTaxa<FlatTaxon>().Where(t => t.Id == tagId).SingleOrDefault();

                    if (taxon == null)
                    {
                        taxon = manager.CreateTaxon<FlatTaxon>(tagId);

                        taxon.Title = title;
                        taxon.Name = title;
                        taxon.UrlName = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                        tags.Taxa.Add(taxon);

                        manager.SaveChanges();
                    }
                }
            }
        }

        public static void CreateProfileForUser(string username, string firstName, string lastName)
        {
            UserManager userManager = UserManager.GetManager();
            using (new ElevatedModeRegion(userManager))
            {
                User user = userManager.GetUsers().SingleOrDefault(u => u.UserName == username);
                if (user != null)
                {
                    UserProfileManager profileManager = UserProfileManager.GetManager();
                    using (new ElevatedModeRegion(profileManager))
                    {
                        var userProfile = (SitefinityProfile)profileManager.GetUserProfile(user.Id, typeof(SitefinityProfile).FullName);
                        if (userProfile == null)
                        {
                            CreateSitefinityProfile(firstName, lastName, profileManager, user);
                        }
                    }
                }
            }
        }

        private static void CreateSitefinityProfile(string firstName, string lastName, UserProfileManager profileManager, User user)
        {
            var userProfile = profileManager.CreateProfile(user, user.Id, typeof(SitefinityProfile)) as SitefinityProfile;
            if (userProfile != null)
            {
                userProfile.FirstName = firstName;
                userProfile.LastName = lastName;
                profileManager.SaveChanges();
            }
        }

        public static Guid CreateUsersAndRoles()
        {
            var userManager = UserManager.GetManager();
            var userId = Guid.Empty;
            using (new ElevatedModeRegion(userManager))
            {
                string username = SampleUtilities.DefaultUserName;
                string password = SampleUtilities.DefaultUserPassword;
                if (!userManager.UserExists(username))
                {
                    userManager.Provider.SuppressSecurityChecks = true;
                    MembershipCreateStatus status;
                    User user = userManager.CreateUser(username, password, "admin@sample.com", "test", "yes", true, null, out status);
                    if (status != MembershipCreateStatus.Success)
                    {
                        throw new InvalidOperationException("User cannot be created" + status.ToString());
                    }
                    userId = user.Id;
                    userManager.SaveChanges();
                    userManager.Provider.SuppressSecurityChecks = false;

                    CreateProfileForUser(username, SampleUtilities.DefaultUserFirstName, SampleUtilities.DefaultUserLastName);

                    var roleManager = RoleManager.GetManager("AppRoles");
                    using (new ElevatedModeRegion(roleManager))
                    {
                        roleManager.Provider.SuppressSecurityChecks = true;
                        Guid id;
                        foreach (var a in Config.Get<SecurityConfig>().ApplicationRoles.Keys)
                        {
                            var info = Config.Get<SecurityConfig>().ApplicationRoles[a];
                            id = info.Id;
                            var role = roleManager.GetRoles().FirstOrDefault(r => r.Id == id);
                            if (role == null)
                            {
                                roleManager.CreateRole(info.Id, info.Name);
                            }
                        }
                        roleManager.SaveChanges();

                        var adminRole = roleManager.GetRole("Administrators");
                        roleManager.AddUserToRole(user, adminRole);
                        roleManager.SaveChanges();
                        roleManager.Provider.SuppressSecurityChecks = false;
                    }
                }
            }
            return userId;
        }

        public static void FrontEndAuthenticate()
        {
            SecurityManager.Logout();

            string username = SampleUtilities.DefaultUserName;
            string password = SampleUtilities.DefaultUserPassword;
            string issuer = SitefinityClaimsAuthenticationModule.Current.GetIssuer();
            string currentRequestPath = HttpUtility.UrlEncode(HttpContext.Current.Request.Url.AbsolutePath);
            string realm = HttpUtility.UrlEncode(SitefinityClaimsAuthenticationModule.Current.GetRealm());

            var authenticateUrl = "{0}?deflate=true&redirect_uri={1}&wrap_name={2}&wrap_password={3}&realm={4}".Arrange(issuer,
                currentRequestPath, username, password, realm);
            HttpContext.Current.Response.Redirect(authenticateUrl, false);
        }

        public static Guid CreateUser(string userName, string password, string email, string firstName, string lastName, string passwordQuestion, string passwordAnswer, bool isBackendUser)
        {
            var userManager = UserManager.GetManager();
            Guid userId = Guid.NewGuid();

            using (new ElevatedModeRegion(userManager))
            {
                if (!userManager.UserExists(userName))
                {
                    userManager.Provider.SuppressSecurityChecks = true;
                    System.Web.Security.MembershipCreateStatus status;

                    User user = userManager.CreateUser(userName, password, email, passwordQuestion, passwordAnswer, isBackendUser, null, out status);

                    if (status == MembershipCreateStatus.Success)
                    {
                        UserProfileManager profileManager = UserProfileManager.GetManager();
                        using (new ElevatedModeRegion(profileManager))
                        {
                            var sfProfile =
                                profileManager.CreateProfile(user, userId, typeof (SitefinityProfile)) as
                                    SitefinityProfile;

                            if (sfProfile != null)
                            {
                                sfProfile.FirstName = firstName;
                                sfProfile.LastName = lastName;
                            }

                            profileManager.SaveChanges();
                        }
                    }
                }

                userManager.SaveChanges();
                userManager.Provider.SuppressSecurityChecks = false;
            }
            return userId;
        }

        public static void SetUserAvatar(Guid userId, string avatarName)
        {
            var userManager = UserManager.GetManager();
            using (new ElevatedModeRegion(userManager))
            {
                var user = userManager.GetUsers().Where(u => u.Id == userId).FirstOrDefault();

                if (user != null)
                {
                    userManager.Provider.SuppressSecurityChecks = true;

                    UserProfileManager profileManager = UserProfileManager.GetManager();
                    using (new ElevatedModeRegion(profileManager))
                    {
                        SitefinityProfile sfProfile = profileManager.GetUserProfile(user, typeof(SitefinityProfile)) as SitefinityProfile;

                        if (sfProfile != null && !string.IsNullOrEmpty(avatarName))
                        {
                            Guid avatarId = GetLocalizedImageId(avatarName, "en");
                            if (avatarId != Guid.Empty)
                            {
                                var libManager = LibrariesManager.GetManager();
                                using (new ElevatedModeRegion(libManager))
                                {
                                    Image avatarImage = libManager.GetImages().Where(i => i.Id == avatarId).SingleOrDefault();
                                    ContentLink avatarContentLink = ContentLinksExtensions.CreateContentLink(sfProfile, avatarImage);

                                    sfProfile.Avatar = avatarContentLink;
                                }
                            }
                        }

                        profileManager.SaveChanges();
                    }
                    userManager.SaveChanges();
                    userManager.Provider.SuppressSecurityChecks = false;
                }
            }
        }

        public static Guid GetUserIdByUserName(string userName)
        {
            Guid userId = Guid.Empty;

            var userManager = UserManager.GetManager();
            using (new ElevatedModeRegion(userManager))
            {
                var user = userManager.GetUsers().FirstOrDefault(u => u.UserName == userName);
                if (user != null)
                {
                    userId = user.Id;
                }
            }

            return userId;
        }

        public static void CreateApplicationRoles()
        {
            var roleManager = RoleManager.GetManager("AppRoles");
            using (new ElevatedModeRegion(roleManager))
            {
                foreach (var a in Config.Get<SecurityConfig>().ApplicationRoles.Keys)
                {
                    var info = Config.Get<SecurityConfig>().ApplicationRoles[a];
                    if (roleManager.GetRoles().FirstOrDefault(r => r.Id == info.Id) == null)
                    {
                        roleManager.CreateRole(info.Id, info.Name);
                    }
                }

                roleManager.SaveChanges();
            }
        }

        public static void CreateRole(Guid id, string roleName)
        {
            var roleManager = RoleManager.GetManager();
            using (new ElevatedModeRegion(roleManager))
            {
                var role = roleManager.GetRoles().FirstOrDefault(r => r.Id == id);
                if (role == null)
                {
                    roleManager.CreateRole(id, roleName);
                }

                roleManager.SaveChanges();
            }
        }

        public static void AddUserToApplicationRoles(string userName, List<string> appRoles)
        {
            UserManager userManager = UserManager.GetManager();
            using (new ElevatedModeRegion(userManager))
            {
                User user = userManager.GetUser(userName);

                RoleManager roleManager = RoleManager.GetManager("AppRoles");
                using (new ElevatedModeRegion(roleManager))
                {
                    foreach (var roleName in appRoles)
                    {
                        Role role = roleManager.GetRole(roleName);
                        roleManager.AddUserToRole(user, role);
                    }

                    roleManager.SaveChanges();
                }
            }
        }

        public static string GetControlTemplateKey(Type controlType, string templateName)
        {
            string templateKey = string.Empty;

            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var layoutTemplates = pageManager.GetPresentationItems<ControlPresentation>()
                .Where(t => t.DataType == Presentation.AspNetTemplate);
                int? totalCount = 0;
                var filterExpression = String.Format(@"ControlType == ""{0}""", controlType);
                layoutTemplates = DataProviderBase.SetExpressions(layoutTemplates, filterExpression, String.Empty, 0, 0, ref totalCount);

                var template = layoutTemplates.Where(t => t.Name == templateName).FirstOrDefault();

                if (template != null)
                {
                    templateKey = template.Id.ToString();
                }
            }
            return templateKey;
        }

        public static string GetImageDefaultUrl(string imageName)
        {
            var librariesManager = LibrariesManager.GetManager();

            string imageUrl = string.Empty;
            using (new ElevatedModeRegion(librariesManager))
            {
                var image = librariesManager.GetImages().Where(i => i.Title == imageName && i.Status == ContentLifecycleStatus.Live).SingleOrDefault();

                if (image != null)
                {
                    var url = image.Urls.FirstOrDefault();

                    if (url != null)
                    {
                        imageUrl = url.Url + image.Extension;
                    }
                }
            }
            return imageUrl;
        }

        public static Guid GetImageId(string imageName)
        {
            var librariesManager = LibrariesManager.GetManager();
            Guid imageId = Guid.Empty;
            using (new ElevatedModeRegion(librariesManager))
            {
                var image = librariesManager.GetImages().Where(i => i.Title == imageName && i.Status == ContentLifecycleStatus.Live).SingleOrDefault();

                if (image != null)
                {
                    imageId = image.Id;
                }
            }
            return imageId;
        }

        public static Guid GetLocalizedImageId(string imageName, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var librariesManager = LibrariesManager.GetManager();
            Guid imageId = Guid.Empty;
            using (new ElevatedModeRegion(librariesManager))
            {
                var whereClause = @"Title[""" + culture + @"""] == """ + imageName + @"""";
                Image image = librariesManager.GetImages().Where(whereClause).Where(i => i.Status == ContentLifecycleStatus.Live).SingleOrDefault();

                if (image != null)
                {
                    imageId = image.Id;
                }
            }
            Thread.CurrentThread.CurrentUICulture = currentCulture;

            return imageId;
        }

        public static Guid GetLocalizedImageMasterId(string imageName, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var librariesManager = LibrariesManager.GetManager();
            Guid imageId = Guid.Empty;
            using (new ElevatedModeRegion(librariesManager))
            {
                var whereClause = @"Title[""" + culture + @"""] == """ + imageName + @"""";
                Image image = librariesManager.GetImages().Where(whereClause).Where(i => i.Status == ContentLifecycleStatus.Master).SingleOrDefault();

                if (image != null)
                {
                    imageId = image.Id;
                }
            }
            Thread.CurrentThread.CurrentUICulture = currentCulture;

            return imageId;
        }

        public static string GetLocalizedImageDefaultUrl(string imageName, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var librariesManager = LibrariesManager.GetManager();
            string imageUrl = string.Empty;
            using (new ElevatedModeRegion(librariesManager))
            {
                var whereClause = @"Title[""" + culture + @"""] == """ + imageName + @"""";
                Image image = librariesManager.GetImages().Where(whereClause).Where(i => i.Status == ContentLifecycleStatus.Live).SingleOrDefault();

                if (image != null)
                {
                    imageUrl = image.Url;
                }
            }
            Thread.CurrentThread.CurrentUICulture = currentCulture;

            return imageUrl;
        }

        public static void RegisterAspNetControlTemplate(string name, string embeddedResourcePath, string resourceAssemblyName, Type controlType)
        {
            RegisterControlTemplate(name, embeddedResourcePath, resourceAssemblyName, controlType, null, null, Presentation.AspNetTemplate, null);
        }

        public static void RegisterControlTemplate(string name, string embeddedResourcePath, string resourceAssemblyName, Type controlType, string condition, string areaName, string dataType, string friendlyControlName)
        {
            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                if (pageManager.GetPresentationItems<ControlPresentation>().
                    Where(t => t.EmbeddedTemplateName == embeddedResourcePath && t.ResourceAssemblyName == resourceAssemblyName &&
                               t.DataType == dataType && t.Name == name && t.ControlType == controlType.FullName && t.Condition == condition).FirstOrDefault() != null)
                    return;

                var template = pageManager.CreatePresentationItem<ControlPresentation>(Guid.NewGuid());
                template.DataType = dataType;
                template.Name = name;
                template.ControlType = controlType.FullName;
                template.Condition = condition;

                SetAreaAndFriendlyName(template, controlType, areaName, friendlyControlName);

                template.EmbeddedTemplateName = embeddedResourcePath;
                template.ResourceAssemblyName = resourceAssemblyName;
                template.IsDifferentFromEmbedded = false;

                pageManager.SaveChanges();
            }
        }

        public static void RegisterControl(string controlName, Type controlType, string toolboxName, string sectionName)
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var config = configManager.GetSection<ToolboxesConfig>();

                var controls = config.Toolboxes[toolboxName];
                var section = controls.Sections.Where<ToolboxSection>(e => e.Name == sectionName).FirstOrDefault();

                if (section == null)
                {
                    section = new ToolboxSection(controls.Sections)
                    {
                        Name = sectionName,
                        Title = sectionName,
                        Description = sectionName,
                        ResourceClassId = typeof(PageResources).Name
                    };
                    controls.Sections.Add(section);
                }

                if (!section.Tools.Any<ToolboxItem>(e => e.Name == controlName))
                {
                    var tool = new ToolboxItem(section.Tools)
                    {
                        Name = controlName,
                        Title = controlName,
                        Description = controlName,
                        ControlType = controlType.AssemblyQualifiedName
                    };
                    section.Tools.Add(tool);
                }

                configManager.SaveSection(config);
            }
        }

        public static void RegisterControl(string controlName, string controlPath, string toolboxName, string sectionName)
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var config = configManager.GetSection<ToolboxesConfig>();

                var controls = config.Toolboxes[toolboxName];
                var section = controls.Sections.Where<ToolboxSection>(e => e.Name == sectionName).FirstOrDefault();

                if (section == null)
                {
                    section = new ToolboxSection(controls.Sections)
                    {
                        Name = sectionName,
                        Title = sectionName,
                        Description = sectionName,
                        ResourceClassId = typeof(PageResources).Name
                    };
                    controls.Sections.Add(section);
                }

                if (!section.Tools.Any<ToolboxItem>(e => e.Name == controlName))
                {
                    var tool = new ToolboxItem(section.Tools)
                    {
                        Name = controlName,
                        Title = controlName,
                        Description = controlName,
                        ControlType = controlPath
                    };
                    section.Tools.Add(tool);
                }

                configManager.SaveSection(config);
            }
        }

        public static void RegisterFormWidget(string controlName, Type controlType, string sectionName)
        {
            RegisterControl(controlName, controlType, "FormControls", sectionName);
        }

        public static void RegisterModule<T>(string name, string description) where T : ModuleBase
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var config = configManager.GetSection<SystemConfig>();

                var appModulesConfig = config.ApplicationModules;
                if (!appModulesConfig.ContainsKey(name))
                {
                    appModulesConfig.Add(new AppModuleSettings(appModulesConfig)
                    {
                        Name = name,
                        Title = name,
                        Type = typeof(T).FullName,
                        StartupType = StartupType.OnApplicationStart,
                        Description = description
                    });
                }

                configManager.SaveSection(config);
            }
        }

        public static bool RegisterTemplate(Guid id, string name, string title, string masterPage, string theme)
        {
            bool result = false;

            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var template = pageManager.GetTemplates().Where(t => t.Id == id).SingleOrDefault();

                if (template == null)
                {
                    var pageTemplate = pageManager.CreateTemplate(id);

                    pageTemplate.Name = name;
                    pageTemplate.Title = title;
                    //pageTemplate.MasterPage = masterPage;
                    //pageTemplate.Themes = theme;
                    pageTemplate.Category = SiteInitializer.CustomTemplatesCategoryId;

                    var master = pageManager.EditTemplate(pageTemplate.Id);
                    var temp = pageManager.TemplatesLifecycle.CheckOut(master);
                    temp.MasterPage = masterPage;
                    temp.Theme = theme;

                    master = pageManager.TemplatesLifecycle.CheckIn(temp);
                    master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                    pageManager.TemplatesLifecycle.Publish(master);

                    pageManager.SaveChanges();
                    result = true;
                }
            }

            return result;
        }

        public static bool RegisterTemplate(Guid id, string name, string title, string masterPage, string theme, CultureInfo culture)
        {
            bool result = false;

            var pageManager = PageManager.GetManager();
            using (new ElevatedModeRegion(pageManager))
            {
                var template = pageManager.GetTemplates().Where(t => t.Id == id).SingleOrDefault();

                if (template == null)
                {
                    var pageTemplate = pageManager.CreateTemplate(id);

                    pageTemplate.Name = name;
                    pageTemplate.MasterPage = masterPage;
                    pageTemplate.Category = SiteInitializer.CustomTemplatesCategoryId;

                    pageTemplate.Title[culture] = title;

                    var master = pageManager.EditTemplate(pageTemplate.Id, culture);
                    var temp = pageManager.TemplatesLifecycle.CheckOut(master, culture);
                    master = pageManager.TemplatesLifecycle.CheckIn(temp, culture);
                    // set the theme for the particular language version of the template
                    master.Themes.SetString(culture, theme);
                    master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                    pageManager.TemplatesLifecycle.Publish(master, culture);

                    pageManager.SaveChanges();
                    result = true;
                }
                else
                {
                    if (!template.AvailableCultures.Contains(culture))
                    {
                        template.Title[culture] = title;

                        var master = pageManager.EditTemplate(template.Id, culture);
                        var temp = pageManager.TemplatesLifecycle.CheckOut(master, culture);
                        master = pageManager.TemplatesLifecycle.CheckIn(temp, culture);
                        master.Themes.SetString(culture, theme);
                        master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        pageManager.TemplatesLifecycle.Publish(master, culture);

                        pageManager.SaveChanges();
                        result = true;
                    }
                }
            }
            return result;
        }

        public static void RegisterTheme(string name, string path)
        {
            ConfigManager manager = Config.GetManager();
            using (new ElevatedModeRegion(manager))
            {
                var appearanceConfig = manager.GetSection<AppearanceConfig>();

                if (!appearanceConfig.FrontendThemes.ContainsKey(name))
                {
                    var theme = new ThemeElement(appearanceConfig.FrontendThemes)
                    {
                        Name = name,
                        Path = path
                    };

                    appearanceConfig.FrontendThemes.Add(theme);
                }

                manager.SaveSection(appearanceConfig);
            }
        }

        public static void RegisterBackendTheme(string name, string path)
        {
            ConfigManager manager = Config.GetManager();
            using (new ElevatedModeRegion(manager))
            {
                var appearanceConfig = manager.GetSection<AppearanceConfig>();

                if (!appearanceConfig.BackendThemes.ContainsKey(name))
                {
                    var theme = new ThemeElement(appearanceConfig.BackendThemes)
                    {
                        Name = name,
                        Path = path
                    };

                    appearanceConfig.BackendThemes.Add(theme);
                }

                manager.SaveSection(appearanceConfig);
            }
        }

        public static void RegisterToolboxWidget(string controlName, Type controlType, string sectionName)
        {
            RegisterControl(controlName, controlType, "PageControls", sectionName);
        }

        public static void RegisterLayoutControl(string controlName, string controlPath, string sectionName)
        {
            RegisterControl(controlName, controlPath, "PageLayouts", sectionName);
        }

        public static void RegisterLayoutControl(string controlName, Type controlType, string sectionName)
        {
            RegisterControl(controlName, controlType, "PageLayouts", sectionName);
        }

        public static void RegisterToolboxWidget(string controlName, string controlPath, string sectionName)
        {
            RegisterControl(controlName, controlPath, "PageControls", sectionName);
        }

        public static void RegisterNewsFrontendView(string contentViewControlName, string templateKey, Type viewType, string viewName)
        {
            RegisterNewsFrontendView(contentViewControlName, templateKey, viewType, viewName, 20, true, Guid.Empty);
        }

        public static void RegisterNewsFrontendView(string contentViewControlName, string templateKey, Type viewType, string viewName, int itemsPerPage, bool allowPaging, Guid detailsPageId)
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var newsConfig = configManager.GetSection<NewsConfig>();

                var control = newsConfig.ContentViewControls[contentViewControlName];

                if (!control.ContainsView(viewName))
                {
                    var definition = new ContentViewMasterElement(control.ViewsConfig);
                    definition.TemplateKey = templateKey;
                    definition.ViewName = viewName;
                    definition.ViewType = viewType;
                    definition.FilterExpression = "Visible = true AND Status = Live";
                    definition.SortExpression = "PublicationDate DESC";

                    definition.AllowPaging = allowPaging;
                    definition.ItemsPerPage = itemsPerPage;
                    definition.DetailsPageId = detailsPageId;

                    definition.ResourceClassId = "NewsResources";
                    control.ViewsConfig.Add(definition);

                    configManager.SaveSection(newsConfig);
                }
            }
        }

        public static void RegisterNewsFrontendDetailsView(string contentViewControlName, string templateKey, Type viewType, string viewName, bool enableSocialSharing)
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var newsConfig = configManager.GetSection<NewsConfig>();

                var control = newsConfig.ContentViewControls[contentViewControlName];

                if (!control.ContainsView(viewName))
                {
                    var definition = new ContentViewDetailElement(control.ViewsConfig);
                    definition.TemplateKey = templateKey;
                    definition.ViewName = viewName;
                    definition.ViewType = viewType;
                    definition.EnableSocialSharing = enableSocialSharing;

                    definition.ResourceClassId = "NewsResources";
                    control.ViewsConfig.Add(definition);

                    configManager.SaveSection(newsConfig);
                }
            }
        }

        public static void RegisterVideosFrontendView(string contentViewControlName, string templateKey, Type viewType, string viewName, int itemsPerPage, bool allowPaging, Guid parentLibraryId)
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var librariesConfig = configManager.GetSection<LibrariesConfig>();

                var control = librariesConfig.ContentViewControls[contentViewControlName];

                if (!control.ContainsView(viewName))
                {
                    var definition = new VideosViewMasterElement(control.ViewsConfig);
                    definition.TemplateKey = templateKey;
                    definition.ViewName = viewName;
                    definition.ViewType = viewType;
                    definition.FilterExpression = "Visible = true AND Status = Live";
                    definition.SortExpression = "PublicationDate DESC";

                    definition.AllowPaging = allowPaging;
                    definition.ItemsPerPage = itemsPerPage;
                    definition.ItemsParentId = parentLibraryId;

                    definition.ResourceClassId = "VideosResources";
                    control.ViewsConfig.Add(definition);

                    configManager.SaveSection(librariesConfig);
                }
            }
        }

        public static void RegisterBlogPostsFrontendView(string contentViewControlName, string templateKey, Type viewType, string viewName)
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var blogsConfig = configManager.GetSection<BlogsConfig>();

                var control = blogsConfig.ContentViewControls[contentViewControlName];

                if (!control.ContainsView(viewName))
                {
                    var definition = new ContentViewMasterElement(control.ViewsConfig);
                    definition.TemplateKey = templateKey;
                    definition.ViewName = viewName;
                    definition.ViewType = viewType;
                    definition.FilterExpression = "Visible = true AND Status = Live";
                    definition.SortExpression = "PublicationDate DESC";
                    definition.ResourceClassId = "BlogResources";
                    control.ViewsConfig.Add(definition);

                    configManager.SaveSection(blogsConfig);
                }
            }
        }

        public static void RegisterEventsFrontendView(string contentViewControlName, string templateKey, Type viewType, string viewName)
        {
            RegisterEventsFrontendView(contentViewControlName, templateKey, viewType, viewName, Guid.Empty);
        }

        public static void RegisterEventsFrontendView(string contentViewControlName, string templateKey, Type viewType, string viewName, Guid detailsPageId)
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var eventsConfig = configManager.GetSection<EventsConfig>();

                var control = eventsConfig.ContentViewControls[contentViewControlName];

                if (!control.ContainsView(viewName))
                {
                    var definition = new ContentViewMasterElement(control.ViewsConfig);
                    definition.TemplateKey = templateKey;
                    definition.ViewName = viewName;
                    definition.ViewType = viewType;
                    definition.FilterExpression = "Visible = true AND Status = Live";
                    definition.SortExpression = "PublicationDate DESC";
                    definition.ResourceClassId = "EventsResources";
                    definition.DetailsPageId = detailsPageId;

                    control.ViewsConfig.Add(definition);

                    configManager.SaveSection(eventsConfig);
                }
            }
        }

        public static void RegisterEventsFrontendDetailsView(string contentViewControlName, string templateKey, Type viewType, string viewName)
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var eventsConfig = configManager.GetSection<EventsConfig>();

                var control = eventsConfig.ContentViewControls[contentViewControlName];

                if (!control.ContainsView(viewName))
                {
                    var definition = new ContentViewDetailElement(control.ViewsConfig);
                    definition.TemplateKey = templateKey;
                    definition.ViewName = viewName;
                    definition.ViewType = viewType;
                    definition.ResourceClassId = "EventsResources";
                    control.ViewsConfig.Add(definition);

                    configManager.SaveSection(eventsConfig);
                }
            }
        }

        public static void RegisterDocumentsFrontendView(string contentViewControlName, string templateKey, Type viewType, string viewName)
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var config = configManager.GetSection<LibrariesConfig>();

                var control = config.ContentViewControls[contentViewControlName];

                if (!control.ContainsView(viewName))
                {
                    var definition = new ContentViewMasterElement(control.ViewsConfig);
                    definition.TemplateKey = templateKey;
                    definition.ViewName = viewName;
                    definition.ViewType = viewType;
                    definition.FilterExpression = "Visible = true AND Status = Live";
                    definition.SortExpression = "PublicationDate DESC";
                    definition.ResourceClassId = "DocumentsResources";
                    control.ViewsConfig.Add(definition);

                    configManager.SaveSection(config);
                }
            }
        }

        public static void RegisterImagesFrontendView(string contentViewControlName, string templateKey, Type viewType, string viewName, QueryData additionalFilter)
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var config = configManager.GetSection<LibrariesConfig>();

                var control = config.ContentViewControls[contentViewControlName];

                if (!control.ContainsView(viewName))
                {
                    var definition = new ImagesViewMasterElement(control.ViewsConfig);
                    definition.TemplateKey = templateKey;
                    definition.ViewName = viewName;
                    definition.ViewType = viewType;
                    definition.FilterExpression = "Visible = true AND Status = Live";
                    definition.SortExpression = "PublicationDate DESC";
                    definition.ResourceClassId = "ImagesResources";

                    if (additionalFilter != null)
                    {
                        definition.AdditionalFilter = additionalFilter;
                    }

                    control.ViewsConfig.Add(definition);

                    configManager.SaveSection(config);
                }
            }
        }

        public static void SetTagsToImage(Guid masterImageId, List<string> tags)
        {
            bool tagAdded = false;

            using (var fluent = App.WorkWith())
            {
                var facade = fluent.Image(masterImageId);
                using (new ElevatedModeRegion(facade.GetManager()))
                {
                    facade.Do(i =>
                    {
                        if (tags != null && tags.Count > 0)
                        {
                            foreach (string tag in tags)
                            {
                                var taxManager = TaxonomyManager.GetManager();
                                using (new ElevatedModeRegion(taxManager))
                                {
                                    var taxon = taxManager.GetTaxa<FlatTaxon>().SingleOrDefault(t => t.Name == tag);
                                    if (taxon != null)
                                    {
                                        if (!i.Organizer.TaxonExists("Tags", taxon.Id))
                                        {
                                            i.Organizer.AddTaxa("Tags", taxon.Id);
                                            tagAdded = true;
                                        }
                                    }
                                }
                            }
                        }
                        i.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                    });

                    if (tagAdded)
                    {
                        facade.Publish().SaveChanges();
                    }
                    else
                    {
                        facade.CancelChanges();
                    }
                }
            }
        }

        public static void RegisterVirtualPath(string path, string resourceLocation)
        {
            var configManager = ConfigManager.GetManager();
            using (new ElevatedModeRegion(configManager))
            {
                var virtualPathConfig = configManager.GetSection<VirtualPathSettingsConfig>();
                if (!virtualPathConfig.VirtualPaths.ContainsKey(path))
                {
                    var virtualPathElement = new VirtualPathElement(virtualPathConfig.VirtualPaths)
                    {
                        VirtualPath = path,
                        ResolverName = "EmbeddedResourceResolver",
                        ResourceLocation = resourceLocation
                    };

                    virtualPathConfig.VirtualPaths.Add(virtualPathElement);
                    configManager.SaveSection(virtualPathConfig);
                }
            }
        }

        public static void SetTemplateToPage(Guid pageId, Guid templateId)
        {
            using (var fluent = App.WorkWith())
            {
                var pageIdFacade = fluent.Page(pageId);
                using (new ElevatedModeRegion(pageIdFacade.GetManager()))
                {
                    pageIdFacade.AsStandardPage()
                        .CheckOut()
                        .SetTemplateTo(templateId)
                        .Do(p => { p.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished; })
                        .Publish()
                        .SaveChanges();

                    //delete the draft - we don't need it
                    pageIdFacade.AsStandardPage().CheckOut().UndoDraft().SaveChanges();
                }
            }
        }

        public static void SetTemplateToLocalizedPage(Guid pageId, Guid templateId, string culture)
        {
            using (PageManager pageManager = PageManager.GetManager())
            {
                var cultureInfo = CultureInfo.GetCultureInfo(culture);
                using (new CultureRegion(cultureInfo))
                {
                    using (new ElevatedModeRegion(pageManager))
                    {
                        var initialPageNode = pageManager.GetPageNode(pageId);

                        var pageNode = GetPageNodeForLanguage(initialPageNode.Page, cultureInfo);

                        PageData pageData = pageNode.Page;
                        var master = pageManager.EditPage(pageData.Id);
                        master.TemplateId = templateId;
                        master.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        master = pageManager.PagesLifecycle.CheckIn(master);
                        pageManager.PagesLifecycle.Publish(master);
                        pageManager.SaveChanges();
                    }
                }
            }
        }

        public static void SetBackendTheme(string themeName)
        {
            ConfigManager manager = Config.GetManager();
            using (new ElevatedModeRegion(manager))
            {
                var appearanceConfig = manager.GetSection<AppearanceConfig>();
                appearanceConfig.BackendTheme = themeName;
                manager.SaveSection(appearanceConfig);
            }
        }

        public static void UploadDocuments(string folderPath, string libraryName)
        {
            var manager = LibrariesManager.GetManager();
            using (new ElevatedModeRegion(manager))
            {
                var library = manager.GetDocumentLibraries().SingleOrDefault(l => l.Title == libraryName);

                if (library == null)
                {
                    var myFolder = new DirectoryInfo(folderPath);

                    library = manager.CreateDocumentLibrary();
                    library.Title = libraryName;
                    library.UrlName = Regex.Replace(libraryName.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);

                    manager.RecompileItemUrls<DocumentLibrary>(library);
                    manager.SaveChanges();

                    foreach (var file in myFolder.GetFiles())
                    {
                        var document = manager.CreateDocument();

                        document.Parent = library;
                        document.Title = file.Name;
                        document.Description = file.Name;
                        document.UrlName = file.Name;
                        document.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;

                        manager.Upload(document, file.OpenRead(), file.Extension);
                        manager.RecompileItemUrls<Document>(document);
                        manager.Publish(document);
                        manager.SaveChanges();
                    }
                }
            }
        }

        public static void UploadImages(string folderPath, string albumName)
        {
            int count;
            var albumsFacade = App.WorkWith().Albums();
            using (new ElevatedModeRegion(albumsFacade.GetManager()))
            {
                albumsFacade.Where(a => a.Title == albumName).Count(out count);
            }
            if (count == 0)
            {
                var albumId = Guid.NewGuid();

                var myFolder = new DirectoryInfo(folderPath);
                var albumFacade = App.WorkWith().Album();
                using (new ElevatedModeRegion(albumsFacade.GetManager()))
                {
                    albumFacade.CreateNew(albumId).Do(a =>
                    {
                        a.Title = albumName;
                        a.UrlName = Regex.Replace(albumName.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                    }).SaveChanges();
                }
                foreach (var file in myFolder.GetFiles())
                {
                    var imageId = Guid.Empty;
                    var albumIdFacade = App.WorkWith().Album(albumId);
                    using (new ElevatedModeRegion(albumIdFacade.GetManager()))
                    {
                        albumIdFacade.CreateImage().Do(i =>
                        {
                            imageId = i.Id;
                            i.Title = file.Name;
                            i.UrlName = Regex.Replace(file.Name.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                            i.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                        }).CheckOut().UploadContent(file.OpenRead(), file.Extension).CheckIn().Publish().SaveChanges();
                    }
                }
            }
        }

        public static void UploadLocalizedDocuments(string folderPath, string libraryName, Guid libraryId, List<string> cultures)
        {
            int count;
            var documentLibrariesFacade = App.WorkWith().DocumentLibraries();
            using (new ElevatedModeRegion(documentLibrariesFacade.GetManager()))
            {
                documentLibrariesFacade.Where(l => l.Id == libraryId).Count(out count);
            }
            if (count == 0)
            {
                DirectoryInfo myFolder = new DirectoryInfo(folderPath);
                bool libraryCreated = false;

                foreach (string culture in cultures)
                {
                    var cultureInfo = new CultureInfo(culture);
                    var currentCulture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;

                    if (!libraryCreated)
                    {
                        var documentLibFacade = App.WorkWith().DocumentLibrary();
                        using (new ElevatedModeRegion(documentLibFacade.GetManager()))
                        {
                            documentLibFacade.CreateNew(libraryId).Do(l =>
                            {
                                l.Title[cultureInfo] = libraryName;
                                l.UrlName[cultureInfo] = Regex.Replace(libraryName.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                libraryCreated = true;
                            }).SaveChanges();
                        }
                    }
                    else
                    {
                        var docLibIdFacade = App.WorkWith().DocumentLibrary(libraryId);
                        using (new ElevatedModeRegion(docLibIdFacade.GetManager()))
                        {
                            docLibIdFacade.Do(l =>
                            {
                                l.Title[cultureInfo] = libraryName;
                                l.UrlName[cultureInfo] = Regex.Replace(libraryName.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                            }).SaveChanges();
                        }
                    }

                    Thread.CurrentThread.CurrentUICulture = currentCulture;
                }

                foreach (var file in myFolder.GetFiles())
                {
                    var docId = Guid.Empty;
                    bool docCreated = false;
                    string title = file.Name.Substring(0, file.Name.LastIndexOf(file.Extension));

                    foreach (string culture in cultures)
                    {
                        var cultureInfo = new CultureInfo(culture);
                        var currentCulture = Thread.CurrentThread.CurrentUICulture;
                        Thread.CurrentThread.CurrentUICulture = cultureInfo;

                        if (!docCreated)
                        {
                            var docIdFacade = App.WorkWith().DocumentLibrary(libraryId);
                            using (new ElevatedModeRegion(docIdFacade.GetManager()))
                            {
                                docIdFacade.CreateDocument().Do(d =>
                                {
                                    docId = d.Id;
                                    d.Title[cultureInfo] = title;
                                    d.Description[cultureInfo] = title;
                                    d.Urls.Clear();
                                    d.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                    docCreated = true;
                                    d.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                                }).CheckOut().UploadContent(file.OpenRead(), file.Extension).CheckIn().Publish().SaveChanges();
                            }
                        }
                        else
                        {
                            var docFacade = App.WorkWith().Document(docId);
                            using (new ElevatedModeRegion(docFacade.GetManager()))
                            {
                                docFacade.Do(d =>
                                {
                                    d.Title[cultureInfo] = title;
                                    d.Description[cultureInfo] = title;
                                    d.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                    d.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                                }).Publish().SaveChanges();
                            }
                        }

                        Thread.CurrentThread.CurrentUICulture = currentCulture;
                    }
                }
            }
        }

        public static void UploadLocalizedImages(string folderPath, string albumName, Guid albumId, List<string> cultures)
        {
            var count = 0;
            var albumsFacade = App.WorkWith().Albums();
            using (new ElevatedModeRegion(albumsFacade.GetManager()))
            {
                albumsFacade.Where(a => a.Id == albumId).Count(out count);
            }

            if (count == 0)
            {
                DirectoryInfo myFolder = new DirectoryInfo(folderPath);

                bool albumCreated = false;

                foreach (string culture in cultures)
                {
                    var cultureInfo = new CultureInfo(culture);
                    var currentCulture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;

                    if (!albumCreated)
                    {
                        var albumFacade = App.WorkWith().Album();
                        using (new ElevatedModeRegion(albumFacade.GetManager()))
                        {
                            albumFacade.CreateNew(albumId).Do(a =>
                            {
                                a.Title[cultureInfo] = albumName;
                                a.UrlName[cultureInfo] = Regex.Replace(albumName.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                albumCreated = true;
                            }).SaveChanges();
                        }
                    }
                    else
                    {
                        var albumIdFacade = App.WorkWith().Album(albumId);
                        using (new ElevatedModeRegion(albumIdFacade.GetManager()))
                        {
                            albumIdFacade.Do(a =>
                            {
                                a.Title[cultureInfo] = albumName;
                                a.UrlName[cultureInfo] = Regex.Replace(albumName.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                            }).SaveChanges();
                        }
                    }

                    Thread.CurrentThread.CurrentUICulture = currentCulture;
                }

                foreach (var file in myFolder.GetFiles())
                {
                    var imageId = Guid.Empty;
                    bool imageCreated = false;
                    string title = file.Name.Substring(0, file.Name.LastIndexOf(file.Extension));

                    foreach (string culture in cultures)
                    {
                        var cultureInfo = new CultureInfo(culture);
                        var currentCulture = Thread.CurrentThread.CurrentUICulture;
                        Thread.CurrentThread.CurrentUICulture = cultureInfo;

                        if (!imageCreated)
                        {
                            var albumIdFacade = App.WorkWith().Album(albumId);
                            using (new ElevatedModeRegion(albumIdFacade.GetManager()))
                            {
                                albumIdFacade.CreateImage().Do(i =>
                                {
                                    imageId = i.Id;
                                    i.Title[cultureInfo] = title;
                                    i.Urls.Clear();
                                    i.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                    imageCreated = true;
                                    i.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                                }).CheckOut().UploadContent(file.OpenRead(), file.Extension).CheckIn().Publish().SaveChanges();
                            }
                        }
                        else
                        {
                            var imageFacade = App.WorkWith().Image(imageId);
                            using (new ElevatedModeRegion(imageFacade.GetManager()))
                            {
                                imageFacade.Do(i =>
                                {
                                    i.Title[cultureInfo] = title;
                                    i.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                    i.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                                }).Publish().SaveChanges();
                            }
                        }

                        Thread.CurrentThread.CurrentUICulture = currentCulture;
                    }
                }
            }
        }

        /// <summary>
        /// Uploads all videos from a folder
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="libraryName"></param>
        /// <param name="libraryId"></param>
        /// <param name="cultures"></param>
        /// <param name="thumbnails">Dictionary with key - the name of the video the thumbnail applies to and value - path to the thumbnail</param>
        /// <returns>True if all videos are successfully uploaded</returns>
        public static bool UploadLocalizedVideos(string folderPath, string libraryName, Guid libraryId, List<string> cultures, Dictionary<string, string> thumbnails = null)
        {
            var count = 0;
            bool result = false;
            var videoLibsFacade = App.WorkWith().VideoLibraries();
            using (new ElevatedModeRegion(videoLibsFacade.GetManager()))
            {
                videoLibsFacade.Where(l => l.Id == libraryId).Count(out count);
            }
            var thumbnailKeys = thumbnails != null ? thumbnails.Keys : null;
            if (count == 0)
            {
                var myFolder = new DirectoryInfo(folderPath);

                bool libraryCreated = false;

                foreach (string culture in cultures)
                {
                    var cultureInfo = new CultureInfo(culture);
                    var currentCulture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;

                    if (!libraryCreated)
                    {
                        var videoLibFacade = App.WorkWith().VideoLibrary();
                        using (new ElevatedModeRegion(videoLibsFacade.GetManager()))
                        {
                            videoLibFacade.CreateNew(libraryId).Do(l =>
                            {
                                l.Title[cultureInfo] = libraryName;
                                l.UrlName[cultureInfo] = Regex.Replace(libraryName.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                libraryCreated = true;
                            }).SaveChanges();
                        }
                    }
                    else
                    {
                        var videoLibIdFacade = App.WorkWith().VideoLibrary(libraryId);
                        using (new ElevatedModeRegion(videoLibIdFacade.GetManager()))
                        {
                            videoLibIdFacade.Do(l =>
                            {
                                l.Title[cultureInfo] = libraryName;
                                l.UrlName[cultureInfo] = Regex.Replace(libraryName.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                            }).SaveChanges();
                        }
                    }

                    Thread.CurrentThread.CurrentUICulture = currentCulture;
                }

                foreach (var file in myFolder.GetFiles())
                {
                    var videoId = Guid.Empty;
                    bool videoCreated = false;
                    string title = file.Name.Substring(0, file.Name.LastIndexOf(file.Extension));

                    foreach (string culture in cultures)
                    {
                        var cultureInfo = new CultureInfo(culture);
                        var currentCulture = Thread.CurrentThread.CurrentUICulture;
                        Thread.CurrentThread.CurrentUICulture = cultureInfo;

                        if (!videoCreated)
                        {
                            var videoLibIdFacade = App.WorkWith().VideoLibrary(libraryId);
                            using (new ElevatedModeRegion(videoLibIdFacade.GetManager()))
                            {
                                videoLibIdFacade.CreateVideo().Do(v =>
                                {
                                    videoId = v.Id;

                                    v.Title[cultureInfo] = title;
                                    v.Description[cultureInfo] = title;
                                    v.Urls.Clear();
                                    v.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                    videoCreated = true;
                                    v.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                                }).CheckOut().UploadContent(file.OpenRead(), file.Extension).CheckIn().Publish().SaveChanges();
                            }
                        }
                        else
                        {
                            var videoFacade = App.WorkWith().Video(videoId);
                            using (new ElevatedModeRegion(videoFacade.GetManager()))
                            {
                                videoFacade.Do(v =>
                                {
                                    v.Title[cultureInfo] = title;
                                    v.Description[cultureInfo] = title;
                                    v.UrlName[cultureInfo] = Regex.Replace(title.ToLower(), UrlNameCharsToReplace, UrlNameReplaceString);
                                    v.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                                }).Publish().SaveChanges();
                            }
                        }

                        if (thumbnailKeys != null && thumbnailKeys.Contains(title))
                        {
                            LibrariesManager manager = LibrariesManager.GetManager();
                            using (new ElevatedModeRegion(manager))
                            {
                                FileInfo imageInfo = new FileInfo(thumbnails[title]);
                                var imgData = ReadFully(imageInfo.OpenRead());
                                manager.UpdateThumbnail(videoId, imgData);
                                manager.SaveChanges();
                            }
                        }

                        Thread.CurrentThread.CurrentUICulture = currentCulture;
                    }
                }

                result = true;
            }

            return result;
        }

        public static void UpdateVideoThumbnail(Guid videoId, string imagePath)
        {
            LibrariesManager librariesManager = LibrariesManager.GetManager();

            FileInfo imageInfo = new FileInfo(imagePath);
            using (new ElevatedModeRegion(librariesManager))
            {
                var imgData = ReadFully(imageInfo.OpenRead());
                librariesManager.UpdateThumbnail(videoId, imgData);
                librariesManager.SaveChanges();
            }
            using (var fluent = App.WorkWith())
            {
                var videoFacade = fluent.Video(videoId);
                using (new ElevatedModeRegion(videoFacade.GetManager()))
                {
                    videoFacade.Do(v =>
                    {
                        v.ApprovalWorkflowState.Value = SampleUtilities.ApprovalWorkflowStatePublished;
                    }).Publish().SaveChanges();
                }
            }
        }

        private static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static Guid GetLocalizedVideoMasterId(string name, string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var librariesManager = LibrariesManager.GetManager();
            Guid videoId = Guid.Empty;
            using (new ElevatedModeRegion(librariesManager))
            {
                var whereClause = @"Title[""" + culture + @"""] == """ + name + @"""";
                Video video = librariesManager.GetVideos().Where(whereClause).Where(i => i.Status == ContentLifecycleStatus.Master).SingleOrDefault();

                if (video != null)
                {
                    videoId = video.Id;
                }
            }
            Thread.CurrentThread.CurrentUICulture = currentCulture;

            return videoId;
        }

        public static string GenerateLayoutTemplate(List<ColumnDetails> columns, string wrapperClass)
        {
            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(wrapperClass))
            {
                sb.AppendFormat("<div runat=\"server\" class=\"sf_cols\">\r\n ");
            }
            else
            {
                sb.AppendFormat("<div runat=\"server\" class=\"sf_cols {0}\">\r\n ", wrapperClass);
            }

            int currentColumn = 1;

            int defaultColumnWidth = (100 / columns.Count);

            foreach (var column in columns)
            {
                if (columns.Count == 3 && currentColumn == 2)
                {
                    defaultColumnWidth = 34;
                }
                else if (columns.Count > 5)
                {
                    defaultColumnWidth = 20;
                }
                else
                {
                    defaultColumnWidth = (100 / columns.Count);
                }

                sb.Append(GenerateColumnString(columns.Count, currentColumn, defaultColumnWidth, column.ColumnClass, column.ColumnWidthPercentage, column.PlaceholderId, column.ColumnSpaces));
                currentColumn++;
            }

            sb.Append("</div>");

            return sb.ToString();
        }

        private static string GenerateColumnString(int columnsCount, int columnNumber, int defaultColumnWidth, string columnClass, int customColumnWidthPercentage, string placeholderId, ColumnSpaces columnSpaces)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<div runat=\"server\" class=\"sf_colsOut sf_{0}col{1}_{2}_{3}", columnsCount, columnsCount > 1 ? "s" : string.Empty, columnNumber, defaultColumnWidth);

            if (!String.IsNullOrEmpty(columnClass))
            {
                sb.AppendFormat(" {0}", columnClass);
            }

            sb.Append("\" ");

            if (customColumnWidthPercentage > 0)
            {
                sb.AppendFormat("style=\"width: {0}%\"", customColumnWidthPercentage);
            }

            sb.AppendFormat(">\r\n <div id=\"{0}\" runat=\"server\" class=\"sf_colsIn sf_{1}col{2}_{3}in_{4}\"", placeholderId, columnsCount, columnsCount > 1 ? "s" : string.Empty, columnNumber, defaultColumnWidth);

            if (columnSpaces != null)
            {
                sb.AppendFormat(" style=\"margin: {0}px {1}px {2}px {3}px\"", columnSpaces.Top, columnSpaces.Right, columnSpaces.Bottom, columnSpaces.Left);
            }

            sb.Append(">\r\n </div>\r\n </div>\r\n ");

            return sb.ToString();
        }

        private static Guid GetLastControlInPlaceHolderInPageId(PageNode page, string placeHolder)
        {
            var id = Guid.Empty;

            var controls = new List<PageControl>(page.Page.Controls.Where(c => c.PlaceHolder == placeHolder));

            while (controls.Count > 0)
            {
                PageControl control = controls.SingleOrDefault(c => c.SiblingId == id);
                if (control != null)
                {
                    id = control.Id;

                    controls.Remove(control);
                }
            }

            return id;
        }

        private static Guid GetLastControlInPlaceHolderInPageId(PageDraft page, string placeHolder)
        {
            var id = Guid.Empty;
            PageDraftControl control;

            var controls = new List<PageDraftControl>(page.Controls.Where(c => c.PlaceHolder == placeHolder));

            while (controls.Count > 0)
            {
                control = controls.Where(c => c.SiblingId == id).SingleOrDefault();
                id = control.Id;

                controls.Remove(control);
            }

            return id;
        }

        private static Guid GetLastControlInPlaceHolderInFormId(FormDescription form, string placeHolder)
        {
            var id = Guid.Empty;
            FormControl control;

            var controls = new List<FormControl>(form.Controls.Where(c => c.PlaceHolder == placeHolder));

            while (controls.Count > 0)
            {
                control = controls.SingleOrDefault(c => c.SiblingId == id);
                id = control.Id;

                controls.Remove(control);
            }

            return id;
        }

        private static Guid GetLastControlInPlaceHolderInFormId(FormDraft form, string placeHolder)
        {
            var id = Guid.Empty;
            FormDraftControl control;

            var controls = new List<FormDraftControl>(form.Controls.Where(c => c.PlaceHolder == placeHolder));

            while (controls.Count > 0)
            {
                control = controls.SingleOrDefault(c => c.SiblingId == id);
                if (control != null)
                {
                    id = control.Id;

                    controls.Remove(control);
                }
            }

            return id;
        }

        private static Guid GetLastControlInPlaceHolderInTemplateId(PageTemplate template, string placeHolder)
        {
            var id = Guid.Empty;

            var controls = new List<Telerik.Sitefinity.Pages.Model.TemplateControl>(template.Controls.Where(c => c.PlaceHolder == placeHolder));

            while (controls.Count > 0)
            {
                Telerik.Sitefinity.Pages.Model.TemplateControl control = controls.SingleOrDefault(c => c.SiblingId == id);
                if (control != null)
                {
                    id = control.Id;

                    controls.Remove(control);
                }
            }

            return id;
        }

        private static Guid GetLastControlInPlaceHolderInTemplateId(TemplateDraft template, string placeHolder)
        {
            var id = Guid.Empty;
            TemplateDraftControl control;

            var controls = new List<TemplateDraftControl>(template.Controls.Where(c => c.PlaceHolder == placeHolder));

            while (controls.Count > 0)
            {
                control = controls.Where(c => c.SiblingId == id).SingleOrDefault();
                if (control != null)
                {
                    id = control.Id;

                    controls.Remove(control);
                }
            }

            return id;
        }

        private static PageNode GetPageNodeForLanguage(PageData page, CultureInfo language)
        {
            PageNode result = null;
            if (page != null)
            {
                if (page.LocalizationStrategy == LocalizationStrategy.Split && page.PageLanguageLink != null)
                {
                    foreach (var p in page.PageLanguageLink.LanguageLinks)
                    {
                        if (p.UiCulture == language.Name)
                        {
                            result = p.NavigationNode;
                            break;
                        }
                    }
                }
                else
                {
                    result = page.NavigationNode;
                }
            }
            else
            {
                throw new ArgumentException("You must specify a valid page!");
            }

            return result;
        }

        private static bool LanguageExistsForPage(PageData pageData, CultureInfo language)
        {
            var result = false;

            if (pageData != null)
            {
                if (pageData.AvailableCultures.Where(c => c.DisplayName == language.DisplayName).Count() > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        private static void SetAreaAndFriendlyName(ControlPresentation template, Type type, string areaName, string friendlyControlName)
        {
            var friendlyUserNameAttrType = typeof(ControlTemplateInfoAttribute);
            var friendlyUserNameAttr = TypeDescriptor.GetAttributes(type)[friendlyUserNameAttrType] as ControlTemplateInfoAttribute;
            if (friendlyUserNameAttr != null)
            {
                if (String.IsNullOrEmpty(friendlyUserNameAttr.ResourceClassId))
                {
                    template.AreaName = friendlyUserNameAttr.AreaName;
                    template.FriendlyControlName = friendlyUserNameAttr.ControlDisplayName;
                }
                else
                {
                    template.AreaName = Res.Get(friendlyUserNameAttr.ResourceClassId, friendlyUserNameAttr.AreaName);
                    template.FriendlyControlName = Res.Get(friendlyUserNameAttr.ResourceClassId, friendlyUserNameAttr.ControlDisplayName);
                }
            }

            if (areaName != null)
                template.AreaName = areaName;
            if (friendlyControlName != null)
                template.FriendlyControlName = friendlyControlName;
        }

        private static Mapping CreateMapping(string destinationPropertyName, string sourcePropertyName)
        {
            return CreateMapping(destinationPropertyName, new string[] { sourcePropertyName }, false);
        }

        private static Mapping CreateMapping(string destinationPropertyName, string sourcePropertyName, bool isRequired)
        {
            return CreateMapping(destinationPropertyName, new string[] { sourcePropertyName }, isRequired);
        }

        private static Mapping CreateMapping(string destinationPropertyName, string[] sourcePropertyNames, bool isRequired)
        {
            Mapping mapping = new Mapping();
            mapping.Id = Guid.NewGuid();
            mapping.IsRequired = isRequired;
            mapping.ApplicationName = PublishingManager.GetManager().Provider.ApplicationName;
            mapping.DestinationPropertyName = destinationPropertyName;
            mapping.SourcePropertyNames = sourcePropertyNames;

            return mapping;
        }

        private static SitefinityContentPipeSettings CreateSitefinityContentPipeSettings(PublishingPoint publishingPoint, Type contentType, List<Mapping> mappings, bool isInbound, string pipeName, bool isActive, PipeInvokationMode invocationMode, Guid backLinksPageId)
        {
            var pipeSettings = new SitefinityContentPipeSettings();

            pipeSettings.Id = Guid.NewGuid();
            pipeSettings.BackLinksPageId = backLinksPageId;
            pipeSettings.Mappings.Id = Guid.NewGuid();

            foreach (var mapping in mappings)
            {
                pipeSettings.Mappings.Mappings.Add(mapping);
            }

            pipeSettings.ApplicationName = PublishingManager.GetManager().Provider.ApplicationName;
            pipeSettings.ContentTypeName = contentType.FullName;
            if (AppSettings.CurrentSettings.Multilingual)
            {
                pipeSettings.LanguageIds.Add(AppSettings.CurrentSettings.DefaultFrontendLanguage.Name);
            }
            pipeSettings.IsInbound = isInbound;
            pipeSettings.IsActive = isActive;
            pipeSettings.PipeName = pipeName;
            pipeSettings.PublishingPoint = publishingPoint;
            pipeSettings.InvocationMode = invocationMode;

            return pipeSettings;
        }

        private static RssPipeSettings CreateRssPipeSettings(PublishingPoint publishingPoint, List<Mapping> mappings, bool isInbound, string pipeName, bool isActive, PipeInvokationMode invocationMode, RssContentOutputSetting outputSettings, RssFormatOutputSettings formatSettings, int contentSize, int maxItems)
        {
            var pipeSettings = new RssPipeSettings();

            pipeSettings.Id = Guid.NewGuid();
            pipeSettings.Mappings.Id = Guid.NewGuid();

            foreach (var mapping in mappings)
            {
                pipeSettings.Mappings.Mappings.Add(mapping);
            }

            pipeSettings.ApplicationName = PublishingManager.GetManager().Provider.ApplicationName;
            pipeSettings.IsInbound = isInbound;
            pipeSettings.IsActive = isActive;
            pipeSettings.PipeName = pipeName;
            pipeSettings.PublishingPoint = publishingPoint;
            pipeSettings.InvocationMode = invocationMode;
            pipeSettings.OutputSettings = outputSettings;
            pipeSettings.FormatSettings = formatSettings;
            pipeSettings.ContentSize = contentSize;
            pipeSettings.MaxItems = maxItems;
            pipeSettings.UrlName = Regex.Replace(publishingPoint.Name.ToLowerInvariant(), UrlNameCharsToReplace, UrlNameReplaceString);

            return pipeSettings;
        }

        private static List<SimpleDefinitionField> GetDefaultSimpleDefinitionFields()
        {
            List<SimpleDefinitionField> fields = new List<SimpleDefinitionField>();
            MetaType metaType = PublishingSystemFactory.CreatePublishingPointDataType();

            Type baseType = TypeResolutionService.ResolveType(metaType.BaseClassName);

            foreach (MetaField metaField in metaType.Fields)
            {
                fields.Add(new SimpleDefinitionField()
                {
                    Title = ((string.IsNullOrWhiteSpace(metaField.Title)) ? metaField.FieldName : metaField.Title),
                    Name = metaField.FieldName,
                    ClrType = TypeResolutionService.ResolveType(metaField.ClrType).FullName,
                    DBType = metaField.DBType,
                    SQLDBType = metaField.DBSqlType,
                    MaxLength = metaField.MaxLength,
                    DefaultValue = metaField.DefaultValue,
                    IsMetaField = true
                });
            }
            PropertyDescriptorCollection baseProps = TypeDescriptor.GetProperties(baseType);
            foreach (PropertyDescriptor prop in baseProps)
            {
                fields.Add(new SimpleDefinitionField()
                {
                    Title = (string.IsNullOrWhiteSpace(prop.DisplayName) ? prop.DisplayName : prop.Name),
                    Name = prop.Name,
                    ClrType = prop.PropertyType.FullName,
                    IsMetaField = prop is MetafieldPropertyDescriptor
                });
            }

            return fields;
        }

        private static void SetProperties(object control, Dictionary<string, object> properties)
        {
            if (control != null)
            {
                foreach (var property in properties)
                {
                    var prop = control.GetType().GetProperty(property.Key);

                    if (typeof(ICollection).IsAssignableFrom(prop.PropertyType))
                    {
                        var list = prop.GetValue(control, null);

                        prop.PropertyType.GetMethod("Clear").Invoke(list, null);

                        var collection = property.Value as ICollection;
                        if (collection != null)
                            foreach (var item in collection)
                            {
                                prop.PropertyType.GetMethod("Add").Invoke(list, new[] { item });
                            }
                    }
                    else
                    {
                        prop.SetValue(control, property.Value, null);
                    }
                }
            }
        }

        #region Forum Methods

        public static bool CreateForumGroup(Guid groupId, string title, string description)
        {
            bool result = false;

            var mgr = ForumsManager.GetManager();
            using (new ElevatedModeRegion(mgr))
            {
                var group = mgr.GetGroups().Where(b => b.Title == title).FirstOrDefault();

                if (group == null)
                {
                    group = mgr.CreateGroup(groupId);
                    group.Title = title;
                    group.Description = description;
                    mgr.SaveChanges();
                    result = true;
                }
            }

            return result;
        }

        public static bool CreateForum(Guid forumId, Guid groupId, string title, string description)
        {
            bool result = false;

            var mgr = ForumsManager.GetManager();
            using (new ElevatedModeRegion(mgr))
            {
                var forum = mgr.GetForums().Where(b => b.Title == title).FirstOrDefault();

                if (forum == null)
                {
                    forum = mgr.CreateForum(forumId);
                    forum.Title = title;
                    forum.Description = description;
                    forum.UrlName = Regex.Replace(title.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
                    forum.Group = mgr.GetGroup(groupId);

                    mgr.RecompileItemUrls<Forum>(forum);
                    mgr.SaveChanges();
                    result = true;
                }
            }

            return result;
        }

        public static bool CreateForumThreadFromPost(Guid forumId, Guid threadId, Guid postId, string title, string content)
        {
            bool result = false;

            var mgr = ForumsManager.GetManager();
            ForumThread thread = mgr.CreateThread(threadId);
            thread.Title = title;
            thread.UrlName = Regex.Replace(title.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
            thread.Forum = mgr.GetForum(forumId);
            thread.LastModified = DateTime.UtcNow;
            thread.IsPublished = true;
            mgr.RecompileItemUrls<ForumThread>(thread);

            // create the first post
            ForumPost post = mgr.CreatePost(postId);
            post.Title = title;
            post.Thread = thread;
            post.Content = content;
            post.LastModified = DateTime.UtcNow;
            post.IsPublished = true;

            mgr.SaveChanges();

            return result;
        }

        #endregion

        public static readonly string ApprovalWorkflowStatePublished = "Published";
        public static readonly string DefaultUserName = "admin";
        public static readonly string DefaultUserPassword = "password";
        public static readonly string DefaultUserFirstName = "Telerik";
        public static readonly string DefaultUserLastName = "Developer";
    }

    public class CreateThreadRegion : IDisposable
    {
        public IThread Thread { get; private set; }

        public CreateThreadRegion(ICommentService service,
            bool requireApproval = false, bool requireAuthentication = false, string groupKey = null, string language = "en", string key = null)
        {
            this.InitializeThread(service, typeof(NewsItem).FullName, requireApproval, requireAuthentication, groupKey, language, key);
        }

        public CreateThreadRegion(ICommentService service, string threadType,
            bool requireApproval = false, bool requireAuthentication = false, string groupKey = null, string language = "en", string key = null)
        {
            this.InitializeThread(service, threadType, requireApproval, requireAuthentication, groupKey, language, key);
        }

        private void InitializeThread(ICommentService service, string threadType,
            bool requireApproval = false, bool requireAuthentication = false, string groupKey = null, string language = "en", string key = null)
        {
            this.cs = service;
            var author = new AuthorProxy(ClaimsManager.GetCurrentUserId().ToString());

            if (groupKey == null)
            {
                var groupProxy = new GroupProxy("test name", "TestGroupDescription", author);
                var group = cs.CreateGroup(groupProxy);
                groupKey = group.Key;
                this.deleteGroup = true;
            }

            var threadProxy = new ThreadProxy("Thread Test Title", threadType, groupKey, author) { Language = language, };

            if (!key.IsNullOrEmpty())
                threadProxy.Key = key;

            var thread = cs.GetThreads(new ThreadFilter()).FirstOrDefault();

            if (thread == null)
            {
                this.Thread = cs.CreateThread(threadProxy);
            }
            else
                this.Thread = thread;
        }

        public void Dispose()
        {
            if (deleteGroup)
            {
                cs.DeleteGroup(this.Thread.GroupKey);
            }
            else
            {
                cs.DeleteThread(this.Thread.Key);
            }
        }

        private ICommentService cs;
        private bool deleteGroup;
    }
}
