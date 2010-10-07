﻿
using System;

namespace ICSharpCode.NRefactory.TypeSystem.Implementation
{
	/// <summary>
	/// Default implementation of <see cref="IEvent"/>.
	/// </summary>
	public class DefaultEvent : AbstractMember, IEvent
	{
		Accessibility addAccessibility;
		Accessibility removeAccessibility;
		Accessibility invokeAccessibility;
		
		const ushort FlagCanAdd    = 0x1000;
		const ushort FlagCanRemove = 0x2000;
		const ushort FlagCanInvoke  = 0x4000;
		
		public DefaultEvent(ITypeDefinition declaringTypeDefinition, string name)
			: base(declaringTypeDefinition, name, EntityType.Event)
		{
		}
		
		public bool CanAdd {
			get { return flags[FlagCanAdd]; }
			set {
				CheckBeforeMutation();
				flags[FlagCanAdd] = value;
			}
		}
		
		public bool CanRemove {
			get { return flags[FlagCanRemove]; }
			set {
				CheckBeforeMutation();
				flags[FlagCanRemove] = value;
			}
		}
		
		public bool CanInvoke {
			get { return flags[FlagCanInvoke]; }
			set {
				CheckBeforeMutation();
				flags[FlagCanInvoke] = value;
			}
		}
		
		public Accessibility AddAccessibility {
			get { return addAccessibility; }
			set {
				CheckBeforeMutation();
				addAccessibility = value;
			}
		}
		
		public Accessibility RemoveAccessibility {
			get { return removeAccessibility; }
			set {
				CheckBeforeMutation();
				removeAccessibility = value;
			}
		}
		
		public Accessibility InvokeAccessibility {
			get { return invokeAccessibility; }
			set {
				CheckBeforeMutation();
				invokeAccessibility = value;
			}
		}
	}
}
