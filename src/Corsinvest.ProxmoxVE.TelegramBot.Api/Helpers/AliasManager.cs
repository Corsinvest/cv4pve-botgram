/*
 * SPDX-License-Identifier: GPL-3.0-only
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 */

using System.Text.RegularExpressions;
using Corsinvest.ProxmoxVE.Api.Extension.Shell;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.TelegramBot.Api.Helpers;

internal sealed partial record AliasDef(string Name, string Description, string Command, bool System)
{
    public string[] Names => Name.Split(',');

    public bool Exists(string name) => name.Split(',').Any(a => Names.Contains(a));

    public static bool IsValid(string name) => ValidNameRegex().IsMatch(name);

    [GeneratedRegex("^[a-zA-Z0-9,_-]*$")]
    private static partial Regex ValidNameRegex();
}

internal sealed class AliasManager(string fileName)
{
    private readonly List<AliasDef> _alias =
    [
        //cluster
        new("cluster-top,ct,top,❤️", "Cluster top", "get /cluster/resources", true),
        new("cluster-top-node,ctn,topn", "Cluster top for node", "get /cluster/resources type:node", true),
        new("cluster-top-storage,cts,tops", "Cluster top for storage", "get /cluster/resources type:storage", true),
        new("cluster-top-vm,ctv,topv", "Cluster top for VM/CT", "get /cluster/resources type:vm", true),
        new("cluster-status,csts", "Cluster status", "get /cluster/ha/status/current", true),
        new("cluster-replication,crep", "Cluster replication", "get /cluster/replication", true),
        new("cluster-backup,cbck", "Cluster list vzdump backup schedule", "get /cluster/backup", true),
        new("cluster-backup-info,cbckinf", "Cluster info backup schedule", "get /cluster/backup/{backup}", true),

        //node
        new("nodes-list,nlst", "Node services", "get /nodes", true),
        new("node-status,nsts", "Node status", "get /nodes/{node}/status", true),
        new("node-services,nsvc", "Node services", "get /nodes/{node}/services", true),
        new("node-tasks-active,ntact", "Node tasks active", "get /nodes/{node}/tasks source:active", true),
        new("node-tasks-error,nterr", "Node tasks errors", "get /nodes/{node}/tasks errors:1", true),
        new("node-disks-list,ndlst", "Node discks list", "get /nodes/{node}/disks/list", true),
        new("node-version,nver", "Node version", "get /nodes/{node}/version", true),
        new("node-storage,nsto", "Node storage info", "get /nodes/{node}/storage", true),
        new("node-storage-content,nstoc", "Node storage content", "get /nodes/{node}/storage/{storage}/content", true),
        new("node-report,nrpt", "Node report", "get /nodes/{node}/report", true),
        new("node-shutdown,nreb", "Node reboot or shutdown", "create /nodes/{node}/status command:{command}", true),
        new("node-vzdump-list,nvlst", "Node list backup", "get /nodes/{node}/storage/{storage}/content vmid:{vmid} content:backup", true),
        new("node-vzdump-config,nvcfg", "Node Extract configuration from vzdump backup archive", "get /nodes/{node}/vzdump/extractconfig volume:{volume}", true),

        //Qemu
        new("qemu-list,qlst", "Qemu list vm", "get /nodes/{node}/qemu", true),
        new("qemu-exec,qexe", "Qemu exec command vm", "create /nodes/{node}/qemu/{vmid}/agent/exec command:{command}", true),
        new("qemu-migrate,qmig", "Qemu migrate vm other node", "create /nodes/{node}/qemu/{vmid}/migrate target:{target} online:{online}", true),
        new("qemu-vzdump-restore,qvrst", "Qemu restore vzdump", "create /nodes/{node}/qemu vmid:{vmid} archive:{archive}", true),

        //status
        new("qemu-status,qsts", "Qemu current status vm", "get /nodes/{node}/qemu/{vmid}/status/current", true),
        new("qemu-start,qstr", "Qemu start vm", "create /nodes/{node}/qemu/{vmid}/status/start", true),
        new("qemu-stop,qsto", "Qemu stop vm", "create /nodes/{node}/qemu/{vmid}/status/stop", true),
        new("qemu-shutdown,qsdwn", "Qemu shutdown vm", "create /nodes/{node}/qemu/{vmid}/status/shutdown", true),
        new("qemu-config,qcfg", "Qemu config vm", "get /nodes/{node}/qemu/{vmid}/config", true),

        //snapshot
        new("qemu-snap-list,qslst", "Qemu snapshot vm list", "get /nodes/{node}/qemu/{vmid}/snapshot", true),
        new("qemu-snap-create,qscrt", "Qemu snapshot vm create", "create /nodes/{node}/qemu/{vmid}/snapshot snapname:{snapname} description:{description}", true),
        new("qemu-snap-delete,qsdel", "Qemu snapshot vm delete", "delete /nodes/{node}/qemu/{vmid}/snapshot/{snapname}", true),
        new("qemu-snap-config,qscfg", "Qemu snapshot vm delete", "get /nodes/{node}/qemu/{vmid}/snapshot/{snapname}/config", true),
        new("qemu-snap-rollback,qsrbck", "Qemu snapshot vm rollback", "create /nodes/{node}/qemu/{vmid}/snapshot/{snapname}/rollback", true),

        //LXC
        new("lxc-list,llst", "LXC list vm", "get /nodes/{node}/lxc", true),
        new("lxc-migrate,lmig", "LXC migrate vm other node", "create /nodes/{node}/lxc/{vmid}/migrate target:{target} restart:{restart}", true),
        new("lxc-vzdump-restore,lvrst", "LXC restore vzdump", "create /nodes/{node}/lxc vmid:{vmid} ostemplate:{archive} restore:1", true),

        //status
        new("lxc-status,lsts", "LXC current status vm", "get /nodes/{node}/lxc/{vmid}/status/current", true),
        new("lxc-start,lstr", "LXC start vm", "create /nodes/{node}/lxc/{vmid}/status/start", true),
        new("lxc-stop,lsto", "LXC stop vm", "create /nodes/{node}/lxc/{vmid}/status/stop", true),
        new("lxc-shutdown,lsdwn", "LXC shutdown vm", "create /nodes/{node}/lxc/{vmid}/status/shutdown", true),
        new("lxc-config,lcfg", "LXC config vm", "get /nodes/{node}/lxc/{vmid}/config", true),

        //snapshot
        new("lxc-snap-list,lslst", "LXC snapshot vm list", "get /nodes/{node}/lxc/{vmid}/snapshot", true),
        new("lxc-snap-create,lscrt", "LXC snapshot vm create", "create /nodes/{node}/lxc/{vmid}/snapshot snapname:{snapname} description:{description}", true),
        new("lxc-snap-delete,lsdel", "LXC snapshot vm delete", "delete /nodes/{node}/lxc/{vmid}/snapshot/{snapname}", true),
        new("lxc-snap-config,lscfg", "LXC snapshot vm delete", "get /nodes/{node}/lxc/{vmid}/snapshot/{snapname}/config", true),
        new("lxc-snap-rollback,lsrbck", "LXC snapshot vm rollback", "create /nodes/{node}/lxc/{vmid}/snapshot/{snapname}/rollback", true),
    ];

    public IReadOnlyList<AliasDef> Alias => _alias.AsReadOnly();

    public string ToTable(bool verbose, TableGenerator.Output output)
        => TableGenerator.From(_alias.OrderByDescending(a => a.System).ThenBy(a => a.Name))
                         .Column(a => a.Name).Title("name")
                         .Column(a => a.Description).Title("description")
                         .Column(a => a.Command).Title("command").When(verbose)
                         .Column(a => string.Join(",", ApiCommandLine.GetPlaceholders(a.Command))).Title("args").When(verbose)
                         .Column(a => a.System ? "X" : string.Empty).Title("sys")
                         .To(output);

    public bool Create(string name, string description, string command, bool system)
    {
        if (!AliasDef.IsValid(name) || Exists(name)) { return false; }
        _alias.Add(new AliasDef(name, description, command, system));
        return true;
    }

    public bool Exists(string name) => _alias.Any(a => a.Exists(name));

    public bool Remove(string name)
    {
        var item = _alias.FirstOrDefault(a => a.Names.Contains(name) && !a.System);
        if (item != null) { _alias.Remove(item); }
        return item != null;
    }

    public void Load()
    {
        if (!File.Exists(fileName)) { File.WriteAllLines(fileName, []); }

        foreach (var line in File.ReadAllLines(fileName))
        {
            var data = line.Split('\t');
            if (data.Length == 3) { Create(data[0], data[1], data[2], false); }
        }
    }

    public void Save()
        => File.WriteAllLines(fileName, _alias.Where(a => !a.System)
                                              .Select(a => $"{a.Name}\t{a.Description}\t{a.Command}"));
}
