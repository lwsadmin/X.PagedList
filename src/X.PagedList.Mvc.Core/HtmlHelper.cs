using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using X.PagedList.Mvc.Common;


namespace X.PagedList.Mvc.Core
{
    ///<summary>
    ///	.NetCore分页 李文尚
    ///</summary>
    public static class HtmlHelper
    {
        private static void SetInnerText(TagBuilder tagBuilder, string innerText)
        {
            tagBuilder.InnerHtml.SetContent(innerText);
        }

        private static void AppendHtml(TagBuilder tagBuilder, string innerHtml)
        {
            tagBuilder.InnerHtml.AppendHtml(innerHtml);
        }

        private static string TagBuilderToString(TagBuilder tagBuilder, TagRenderMode renderMode = TagRenderMode.Normal)
        {
            var encoder = HtmlEncoder.Create(new TextEncoderSettings());
            var writer = new System.IO.StringWriter() as TextWriter;
            tagBuilder.WriteTo(writer, encoder);
            return writer.ToString();
        }

        private static TagBuilder WrapInListItem(string text)
        {
            var li = new TagBuilder("li");
            SetInnerText(li, text);
            return li;
        }

        private static TagBuilder WrapInListItem(TagBuilder inner, PagedListRenderOptionsBase options, params string[] classes)
        {
            var li = new TagBuilder("li");
            foreach (var @class in classes)
                li.AddCssClass(@class);

            if (options is PagedListRenderOptions)
            {
                if (((PagedListRenderOptions)options).FunctionToTransformEachPageLink != null)
                {
                    inner = ((PagedListRenderOptions)options).FunctionToTransformEachPageLink(li, inner);
                }
            }

            AppendHtml(li, TagBuilderToString(inner));
            return li;
        }

        private static TagBuilder First(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptionsBase options)
        {
            const int targetPageNumber = 1;
            var first = new TagBuilder("a");
            AppendHtml(first, string.Format(options.LinkToFirstPageFormat, targetPageNumber));

            foreach (var c in options.PageClasses ?? Enumerable.Empty<string>())
                first.AddCssClass(c);

            if (list.IsFirstPage)
                return WrapInListItem(first, options, "PagedList-skipToFirst", "disabled");

            first.Attributes["href"] = generatePageUrl(targetPageNumber);
            return WrapInListItem(first, options, "PagedList-skipToFirst");
        }

        private static TagBuilder Previous(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptionsBase options)
        {
            var targetPageNumber = list.PageNumber - 1;
            var previous = new TagBuilder("a");
            AppendHtml(previous, string.Format(options.LinkToPreviousPageFormat, targetPageNumber));
            previous.Attributes["rel"] = "prev";

            foreach (var c in options.PageClasses ?? Enumerable.Empty<string>())
                previous.AddCssClass(c);

            if (!list.HasPreviousPage)
                return WrapInListItem(previous, options, options.PreviousElementClass, "disabled");

            previous.Attributes["href"] = generatePageUrl(targetPageNumber);
            return WrapInListItem(previous, options, options.PreviousElementClass);
        }

        private static TagBuilder Page(int i, IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptionsBase options)
        {
            var format = options.FunctionToDisplayEachPageNumber
                ?? (pageNumber => string.Format(options.LinkToIndividualPageFormat, pageNumber));
            var targetPageNumber = i;
            var page = i == list.PageNumber ? new TagBuilder("a") : new TagBuilder("a");
            SetInnerText(page, format(targetPageNumber));

            foreach (var c in options.PageClasses ?? Enumerable.Empty<string>())
                page.AddCssClass(c);

            if (i == list.PageNumber)
            {
                //当前页的html生成
                page.Attributes["href"] = generatePageUrl(targetPageNumber);
                return WrapInListItem(page, options, options.ActiveLiElementClass);
            }

            page.Attributes["href"] = generatePageUrl(targetPageNumber);

            return WrapInListItem(page, options);
        }

        private static TagBuilder Next(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptionsBase options)
        {
            var targetPageNumber = list.PageNumber + 1;
            var next = new TagBuilder("a");
            AppendHtml(next, string.Format(options.LinkToNextPageFormat, targetPageNumber));
            next.Attributes["rel"] = "next";

            foreach (var c in options.PageClasses ?? Enumerable.Empty<string>())
                next.AddCssClass(c);

            if (!list.HasNextPage)
                return WrapInListItem(next, options, options.NextElementClass, "disabled");

            next.Attributes["href"] = generatePageUrl(targetPageNumber);
            return WrapInListItem(next, options, options.NextElementClass);
        }

        private static TagBuilder Last(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptionsBase options)
        {
            var targetPageNumber = list.PageCount;
            var last = new TagBuilder("a");
            AppendHtml(last, string.Format(options.LinkToLastPageFormat, targetPageNumber));

            foreach (var c in options.PageClasses ?? Enumerable.Empty<string>())
                last.AddCssClass(c);

            if (list.IsLastPage)
                return WrapInListItem(last, options, "PagedList-skipToLast", "disabled");

            last.Attributes["href"] = generatePageUrl(targetPageNumber);
            return WrapInListItem(last, options, "PagedList-skipToLast");
        }

        private static TagBuilder PageCountAndLocationText(IPagedList list, PagedListRenderOptionsBase options)
        {
            var text = new TagBuilder("a");
            SetInnerText(text, string.Format(options.PageCountAndCurrentLocationFormat, list.PageNumber, list.PageCount,list.TotalItemCount));

            return WrapInListItem(text, options, "PagedList-pageCountAndLocation", "disabled");
        }

        private static TagBuilder ItemSliceAndTotalText(IPagedList list, PagedListRenderOptionsBase options)
        {
            var text = new TagBuilder("a");
            SetInnerText(text, string.Format(options.ItemSliceAndTotalFormat, list.FirstItemOnPage, list.LastItemOnPage, list.TotalItemCount));

            return WrapInListItem(text, options, "PagedList-pageCountAndLocation", "disabled");
        }

        private static TagBuilder Ellipses(PagedListRenderOptionsBase options)
        {
            var a = new TagBuilder("a");
            AppendHtml(a, options.EllipsesFormat);

            return WrapInListItem(a, options, options.EllipsesElementClass, "disabled");
        }

        ///<summary>
        ///	显示PagedList实例的可配置分页控件.
        ///</summary>
        ///<param name = "html">这个方法是HtmlHelper作为一个扩展方法.</param>
        ///<param name = "list">The PagedList的数据源 to use as the data source.</param>
        ///<param name = "generatePageUrl">获取所需页面的页码并返回将加载该页面的url字符串的函数</param>
        ///<returns>输出分页控件HTML</returns>
        public static HtmlString PagedListPager(this IHtmlHelper html,
                                                   IPagedList list,
                                                   Func<int, string> generatePageUrl)
        {
            return PagedListPager(html, list, generatePageUrl, new PagedListRenderOptions());
        }

        ///<summary>
        ///	显示PagedList实例的可配置分页控件
        ///</summary>
        ///<param name = "html">这个方法是HtmlHelper作为一个扩展方法</param>
        ///<param name = "list">PagedList的数据源</param>
        ///<param name = "generatePageUrl">获取所需页面的页码并返回将加载该页面的url字符串的函数</param>
        ///<param name = "options">格式化选项</param>
        ///<returns>输出分页控件HTML</returns>
        public static HtmlString PagedListPager(this IHtmlHelper html,
                                                   IPagedList list,
                                                   Func<int, string> generatePageUrl,
                                                   PagedListRenderOptionsBase options)
        {
            if (options.Display == PagedListDisplayMode.Never || (options.Display == PagedListDisplayMode.IfNeeded && list.PageCount <= 1))
                return null;

            var listItemLinks = new List<TagBuilder>();

            //calculate start and end of range of page numbers
            var firstPageToDisplay = 1;
            var lastPageToDisplay = list.PageCount;
            var pageNumbersToDisplay = lastPageToDisplay;
            if (options.MaximumPageNumbersToDisplay.HasValue && list.PageCount > options.MaximumPageNumbersToDisplay)
            {
                // cannot fit all pages into pager
                var maxPageNumbersToDisplay = options.MaximumPageNumbersToDisplay.Value;
                firstPageToDisplay = list.PageNumber - maxPageNumbersToDisplay / 2;
                if (firstPageToDisplay < 1)
                    firstPageToDisplay = 1;
                pageNumbersToDisplay = maxPageNumbersToDisplay;
                lastPageToDisplay = firstPageToDisplay + pageNumbersToDisplay - 1;
                if (lastPageToDisplay > list.PageCount)
                    firstPageToDisplay = list.PageCount - maxPageNumbersToDisplay + 1;
            }

            //first
            if (options.DisplayLinkToFirstPage == PagedListDisplayMode.Always || (options.DisplayLinkToFirstPage == PagedListDisplayMode.IfNeeded && firstPageToDisplay > 1))
                listItemLinks.Add(First(list, generatePageUrl, options));

            //previous
            if (options.DisplayLinkToPreviousPage == PagedListDisplayMode.Always || (options.DisplayLinkToPreviousPage == PagedListDisplayMode.IfNeeded && !list.IsFirstPage))
                listItemLinks.Add(Previous(list, generatePageUrl, options));

            //text
            if (options.DisplayPageCountAndCurrentLocation)
                listItemLinks.Add(PageCountAndLocationText(list, options));

            //text
            if (options.DisplayItemSliceAndTotal)
                listItemLinks.Add(ItemSliceAndTotalText(list, options));

            //page
            if (options.DisplayLinkToIndividualPages)
            {
                //if there are previous page numbers not displayed, show an ellipsis
                if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && firstPageToDisplay > 1)
                    listItemLinks.Add(Ellipses(options));

                foreach (var i in Enumerable.Range(firstPageToDisplay, pageNumbersToDisplay))
                {
                    //show delimiter between page numbers
                    if (i > firstPageToDisplay && !string.IsNullOrWhiteSpace(options.DelimiterBetweenPageNumbers))
                        listItemLinks.Add(WrapInListItem(options.DelimiterBetweenPageNumbers));

                    //show page number link
                    listItemLinks.Add(Page(i, list, generatePageUrl, options));
                }

                //if there are subsequent page numbers not displayed, show an ellipsis
                if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && (firstPageToDisplay + pageNumbersToDisplay - 1) < list.PageCount)
                    listItemLinks.Add(Ellipses(options));
            }

            //next
            if (options.DisplayLinkToNextPage == PagedListDisplayMode.Always || (options.DisplayLinkToNextPage == PagedListDisplayMode.IfNeeded && !list.IsLastPage))
                listItemLinks.Add(Next(list, generatePageUrl, options));

            //last
            if (options.DisplayLinkToLastPage == PagedListDisplayMode.Always || (options.DisplayLinkToLastPage == PagedListDisplayMode.IfNeeded && lastPageToDisplay < list.PageCount))
                listItemLinks.Add(Last(list, generatePageUrl, options));

            if (listItemLinks.Any())
            {
                //append class to first item in list?
                if (!string.IsNullOrWhiteSpace(options.ClassToApplyToFirstListItemInPager))
                    listItemLinks.First().AddCssClass(options.ClassToApplyToFirstListItemInPager);

                //append class to last item in list?
                if (!string.IsNullOrWhiteSpace(options.ClassToApplyToLastListItemInPager))
                    listItemLinks.Last().AddCssClass(options.ClassToApplyToLastListItemInPager);

                //append classes to all list item links
                foreach (var li in listItemLinks)
                    foreach (var c in options.LiElementClasses ?? Enumerable.Empty<string>())
                        li.AddCssClass(c);
            }

            //collapse all of the list items into one big string
            var listItemLinksString = listItemLinks.Aggregate(
                new StringBuilder(),
                (sb, listItem) => sb.Append(TagBuilderToString(listItem)),
                sb => sb.ToString()
                );

            var ul = new TagBuilder("ul");
            AppendHtml(ul, listItemLinksString);
            foreach (var c in options.UlElementClasses ?? Enumerable.Empty<string>())
                ul.AddCssClass(c);

            if (options.UlElementattributes != null)
            {
                foreach (var c in options.UlElementattributes)
                    ul.MergeAttribute(c.Key, c.Value);
            }

            var outerDiv = new TagBuilder("div");
            foreach (var c in options.ContainerDivClasses ?? Enumerable.Empty<string>())
            {
                outerDiv.AddCssClass(c);
            }
            AppendHtml(outerDiv, TagBuilderToString(ul));
            if (options.IsAjax)
            {
                StringBuilder sb_go = new StringBuilder();
                sb_go.Append("<div class='col-md-2' style='margin: 20px 0;'><div class='input-group'>" +
                    "<input type='number' min='1' max='"+ list.PageCount.ToString() + "' class='form-control input-sm' value='" + list.PageNumber.ToString() +
                "'>");
                sb_go.Append(string.Format(@"<span class='input-group-btn'> <button onclick='GoPage(this);' type='button' class='btn btn-primary btn-sm'>Go
                                        </button ></span></div></div>"));
                AppendHtml(outerDiv, sb_go.ToString());
            }

            string s = TagBuilderToString(outerDiv);
            return new HtmlString(s);
        }

        ///<summary>
        /// 显示跳转到某页.
        ///</summary>
        ///<param name="html">为了输出HtmlHelper作为一个扩展方法</param>
        ///<param name="list">分页数据源.</param>
        ///<param name="formAction">GET请求提交到的URL.</param>
        ///<returns>输出跳转到某页.</returns>
        public static HtmlString PagedListGoToPageForm(this IHtmlHelper html, IPagedList list, string formAction)
        {
            return PagedListGoToPageForm(html, list, formAction, "page");
        }

        ///<summary>
        /// 显示跳转到某页
        ///</summary>
        ///<param name="html">这个方法是HtmlHelper作为一个扩展方法.</param>
        ///<param name="list">分页的数据源</param>
        ///<param name="formAction">GET请求提交到的URL.</param>
        ///<param name="inputFieldName">The querystring key this form should submit the new page number as.</param>
        ///<returns>输出跳转到某页.</returns>
        public static HtmlString PagedListGoToPageForm(this IHtmlHelper html,
                                                          IPagedList list,
                                                          string formAction,
                                                          string inputFieldName)
        {
            return PagedListGoToPageForm(html, list, formAction, new GoToFormRenderOptions(inputFieldName));
        }

        ///<summary>
        /// 显示跳转到某页.
        ///</summary>
        ///<param name="html">为了输出HtmlHelper作为一个扩展方法</param>
        ///<param name="list">分页数据源.</param>
        ///<param name="formAction">GET请求提交到的URL.</param>
        ///<param name="options">格式化选项.</param>
        ///<returns>输出跳转到某页.</returns>
        public static HtmlString PagedListGoToPageForm(this IHtmlHelper html,
                                                 IPagedList list,
                                                 string formAction,
                                                 GoToFormRenderOptions options)
        {
            var form = new TagBuilder("form");
            form.AddCssClass("PagedList-goToPage");
            form.Attributes.Add("action", formAction);
            form.Attributes.Add("method", "get");

            var fieldset = new TagBuilder("fieldset");

            var label = new TagBuilder("label");
            label.Attributes.Add("for", options.InputFieldName);
            SetInnerText(label, options.LabelFormat);

            var input = new TagBuilder("input");
            input.Attributes.Add("type", options.InputFieldType);
            input.Attributes.Add("name", options.InputFieldName);
            input.Attributes.Add("value", list.PageNumber.ToString());
            input.Attributes.Add("class", options.InputFieldClass);
            input.Attributes.Add("Style", $"width: {options.InputWidth}px");

            var submit = new TagBuilder("input");
            submit.Attributes.Add("type", "submit");
            submit.Attributes.Add("value", options.SubmitButtonFormat);
            submit.Attributes.Add("class", options.SubmitButtonClass);
            submit.Attributes.Add("Style", $"width: {options.SubmitButtonWidth}px");

            AppendHtml(fieldset, TagBuilderToString(label));

            AppendHtml(fieldset, TagBuilderToString(input, TagRenderMode.SelfClosing));

            AppendHtml(fieldset, TagBuilderToString(submit, TagRenderMode.SelfClosing));

            AppendHtml(form, TagBuilderToString(fieldset));
            return new HtmlString(TagBuilderToString(form));
        }
    }
}