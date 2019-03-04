using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Catel.Data;
using SharedModels.Collections;
using SharedModels.Data;
using SharedModels.Extensions;
using CatelModelBase = Catel.Data.ModelBase;

namespace SharedModels
{
    public abstract partial class TrackableModelBase : ModelBase, IEditableObject
    {
        public static bool IsLoadingFromDatabase = false;
        
        protected override bool ShouldPropertyChangeUpdateIsDirty(string propertyName)
        {
            return false;
        }

        [NotMapped]
        private Dictionary<string, CollectionChangeNotificationWrapper> CollectionTrackers = new Dictionary<string, CollectionChangeNotificationWrapper>();
        
        private static HashSet<string> _ignoredPropertiesForModifiedProperties =
            new HashSet<string> {nameof(ModifiedProperties), nameof(TrackingState), nameof(IsDirty), nameof(IsUserModified), nameof(IsEditing)};

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.IsNewValueMeaningful && e.NewValue != e.OldValue)
            {
            // Throw away an old collection tracker
            if (CollectionTrackers.ContainsKey(e.PropertyName))
            {
                if (e.OldValue is ITrackableCollection && CollectionTrackers.ContainsKey(e.PropertyName))
                {
                    CollectionTrackers[e.PropertyName].Unsubscribe();
                    CollectionTrackers[e.PropertyName].CollectionChanged -= OnWrappedCollectionChanged;
                    CollectionTrackers[e.PropertyName].CollectionItemPropertyChanged -= OnCollectionItemPropertyChanged;
                }
            }
            
            if (e.NewValue != null && e.NewValue is ITrackableCollection value)
            {
                CollectionTrackers[e.PropertyName] = new CollectionChangeNotificationWrapper(value, e.PropertyName);
                CollectionTrackers[e.PropertyName].CollectionChanged += OnWrappedCollectionChanged;
                CollectionTrackers[e.PropertyName].CollectionItemPropertyChanged += OnCollectionItemPropertyChanged;
            }
            
                MarkPropertyModified(e.PropertyName);
            }
        }

        private void OnCollectionItemPropertyChanged(object sender, WrappedCollectionItemPropertyChangedEventArgs e)
        {
            MarkPropertyModified(e.SourceProperty);
        }

        private void OnWrappedCollectionChanged(object sender, WrappedCollectionChangedEventArgs e)
        {
            MarkPropertyModified(e.SourceProperty);
        }

        private void MarkPropertyModified(string propertyName)
        {
            if (IsLoadingFromDatabase)
            {
                return;
            }
            
            if (ModifiedProperties == null)
            {
                ModifiedProperties = new HashSet<string>();
            }
            
            if (!ModifiedProperties.Contains(propertyName) &&
                !_ignoredPropertiesForModifiedProperties.Contains(propertyName))
            {
                ModifiedProperties.Add(propertyName);
            }

            if (ShouldTrackUserChanges())
            {
                RecalculateIsUserModified(propertyName);
            }
        }
    }
}