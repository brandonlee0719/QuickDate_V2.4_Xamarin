using System;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace QuickDate.Library.Anjo.IntegrationRecyclerView
{
    public class RecyclerToListViewScrollListener : RecyclerView.OnScrollListener
    {
        public static readonly int ApiUnknownScrollState = int.MinValue;
        private readonly AbsListView.IOnScrollListener ScrollListener;
        private int LastFirstVisible = -1;
        private int LastVisibleCount = -1;
        private int LastItemCount = -1;

        public RecyclerToListViewScrollListener(AbsListView.IOnScrollListener scrollListener)
        {
            this.ScrollListener = scrollListener;
        }

        public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            base.OnScrollStateChanged(recyclerView, newState);
            ScrollState listViewState = ScrollState.TouchScroll; //wael;

            switch (newState)
            {
                case RecyclerView.ScrollStateDragging:
                    listViewState = ScrollState.TouchScroll;
                    break;
                case RecyclerView.ScrollStateIdle:
                    listViewState = ScrollState.Idle;
                    break;
                case RecyclerView.ScrollStateSettling:
                    listViewState = ScrollState.Fling;
                    break;
            }

            ScrollListener.OnScrollStateChanged(null /*view*/, listViewState);

        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);

            LinearLayoutManager layoutManager = (LinearLayoutManager)recyclerView.GetLayoutManager();

            int firstVisible = layoutManager.FindFirstVisibleItemPosition();
            int visibleCount = Math.Abs(firstVisible - layoutManager.FindLastVisibleItemPosition());
            int itemCount = recyclerView.GetAdapter().ItemCount;

            if (firstVisible != LastFirstVisible
                || visibleCount != LastVisibleCount
                || itemCount != LastItemCount)
            {
                ScrollListener.OnScroll(null, firstVisible, visibleCount, itemCount);
                LastFirstVisible = firstVisible;
                LastVisibleCount = visibleCount;
                LastItemCount = itemCount;
            }
        }

    }

}